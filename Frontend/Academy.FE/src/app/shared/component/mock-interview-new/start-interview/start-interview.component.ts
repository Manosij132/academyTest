import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  AfterViewInit,
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { HttpClient, HttpContext } from "@angular/common/http";
import { CommonModule } from "@angular/common";

// Material modules
import { MatCardModule } from "@angular/material/card";
import { MatListModule } from "@angular/material/list";
import { MatButtonModule } from "@angular/material/button";
import { MatSidenavModule } from "@angular/material/sidenav";
import { MatIconModule } from "@angular/material/icon";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatChipsModule } from "@angular/material/chips";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatDividerModule } from "@angular/material/divider";
import { MatToolbarModule } from "@angular/material/toolbar";

import { SpeedoMeterComponent } from "../speedo-meter/speedo-meter.component";
import { InterviewsService } from "../../../../services/interviews.service";
import { Profile, ProfileService } from "../../../../services/profile.service";
import {
  Skills,
  SkillsServiceService,
} from "../../../../services/skills.service";
import { environment } from "../../../../../environments/environment";
import { DialogData } from "../common-dialog/models/dialog-data.model";
import { MatDialog } from "@angular/material/dialog";
import { CommonDialogComponent } from "../common-dialog/common-dialog.component";
import { LoaderService } from "../../../../services/loader.service";
import { DomSanitizer, SafeHtml } from "@angular/platform-browser";
import { SKIP_LOADER } from "../../../../context/skip-loader.context";
import {
  nextQuestionConfirmationDialogue,
  pauseInteviewConfirmationDialogue,
} from "../mock-interview-constants";
import { FormsModule } from "@angular/forms";
import { SelfRatingComponent } from "@shared/component/self-rating/self-rating.component";
import { take } from "rxjs";

@Component({
  selector: "app-start-interview",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatButtonModule,
    MatSidenavModule,
    MatChipsModule,
    MatCheckboxModule,
    FormsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatToolbarModule,
    SpeedoMeterComponent,
    SelfRatingComponent
  ],
  templateUrl: "./start-interview.component.html",
  styleUrl: "./start-interview.component.css",
})
export class StartInterviewComponent implements OnInit, OnDestroy {
  private mergeTriggered = false;
  private scoreRanges = environment.speedRanges;
  isDialogOpen = false;
  questions: any;

  @ViewChild("avatarVideo") avatarVideo!: ElementRef<HTMLVideoElement>;
  avatarVideoUrl: string = "assets/video/final.mp4"; // Path to your avatar video
  @ViewChild("videoElement") videoElementRef!: ElementRef<HTMLVideoElement>;
  @ViewChild("mixCanvas") mixCanvas!: ElementRef<HTMLCanvasElement>;
  code: string = "";
  interview: any = null;
  profile: Profile | null = null;
  skills: Skills[] = [];
  loading = true;
  error: string | null = null;
  summaryData: any;

  permissionsGranted = false;
  permissionError: string | null = null;

  started = false;
  paused = false;
  currentQuestionIndex = 0;
  currentQuestion: any = null;
  askedQuestions: { text: string; section: number; question: number }[] = [];

  mediaRecorder: MediaRecorder | null = null;
  recordedChunks: Blob[] = [];
  stream: MediaStream | null = null;
  micStream: MediaStream | null = null;
  canvasStream: MediaStream | null = null;
  pauseCountdown: number | null = null;

  chunkIndex = 0;
  capturedScore: any;
  voices: SpeechSynthesisVoice[] = [];
  selectedVoice?: SpeechSynthesisVoice;
  showInstructions = true;
  answeredQuestionMap: Record<number, number> = {};
  private audioContext!: AudioContext;
  private analyser!: AnalyserNode;
  private micSource!: MediaStreamAudioSourceNode;

  public candidateCanSpeak = false;
  private candidateSpoke = false;
  isHeadphoneConfirmed: boolean = false;
  isTabPolicyConfirmed: boolean = false;
  selfRatingCompleted = false;
  evaluationType: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private profileService: ProfileService,
    private skillsService: SkillsServiceService,
    private interviewService: InterviewsService,
    private dialog: MatDialog,
    private loaderService: LoaderService,
    private sanitizer: DomSanitizer,
    private interviews: InterviewsService,
  ) { }

  ngOnInit() {
    this.checkPermissions();
    this.loadVoices();
    this.showInstructions = true;
    document.addEventListener("visibilitychange", this.handleVisibilityChange);
    speechSynthesis.onvoiceschanged = () => {
      this.loadVoices();
    };

    this.code = this.route.snapshot.paramMap.get("code") || "";
    if (!/^\d{3}-\d{3}-\d{3}-\d{3}$/.test(this.code)) {
      this.error = "Invalid evaluation code.";
      this.loading = false;
      return;
    }

    this.http
      .get(`${environment.apiMockinterviewBaseURL}/interview/code/${this.code}`)
      .subscribe({
        next: (data: any) => {
          this.interview = data;
          if (data?.status === "ended") {
            this.stopRecording();
            this.router.navigate(["/view-interview", this.code]);
          }
          if (data?.profile) {
            this.loaderService.start();
            this.profileService.getById(data.profile?.profileId).subscribe({
              next: (profile) => {
                this.profile = profile;
                const validSkillIds =
                  profile.skillsAndSections?.map((s) => s.skillId) || [];
                this.evaluationType = profile?.fitmentTypeName || '';
                if (validSkillIds?.length > 0) {
                  this.skillsService.getAll().subscribe({
                    next: (allSkills) => {
                      this.loaderService.stop();
                      this.skills = allSkills.filter(
                        (s) =>
                          typeof s.id === "number" &&
                          validSkillIds.includes(s.id),
                      );
                      this.loading = false;
                    },
                    error: () => {
                      this.loading = false;
                    },
                  });
                } else {
                  this.loading = false;
                }
              },
              error: () => {
                this.loading = false;
              },
            });
          } else {
            this.loading = false;
          }
        },
        error: () => {
          this.error = "Evaluation not found.";
          this.loading = false;
          this.askedQuestions = [];
        },
      });
  }

  handleVisibilityChange = () => {
    if (document.visibilityState === "hidden") {
      this.pauseInterview();
    }
  };

  onCancelInterview() {
    // Attempt to close the tab (works if opened via JS)
    window.close();

    // Fallback: redirect to a safe page after a short delay
    setTimeout(() => {
      window.location.href = "/"; // or a "Thank you" page
    }, 100);
  }

  onContinueInterview() {
    this.showInstructions = false;
    setTimeout(() => this.openSelfRatingPopup())
  }

  openSelfRatingPopup(): void {

    const dialogRef = this.dialog.open(SelfRatingComponent, {
      width: '700px',
      disableClose: true,
      data: {
        skills: this.profile?.skillsAndSections
          ?.flatMap(skill =>
            skill.sections
              .filter(section =>
                !section.name?.toLowerCase().includes('experience')
              )
              .map(section => ({
                id: section.id,
                name: section.name,
                rating: 0
              }))
          ) ?? [],
        evaluationType: this.profile?.fitmentTypeName ?? '',
        profileName: this.profile?.profileName ?? ''
      }
    });

    dialogRef.afterClosed()
      .pipe(take(1))
      .subscribe((payload: any) => {

        this.loaderService.start();

        this.interviews.sendRatings(this.code, payload)
          .subscribe({
            next: () => {
              this.loaderService.stop();
              this.selfRatingCompleted = true;
            },
            error: () => {
              this.loaderService.stop();
            }
          });
      });
  }

  ngOnDestroy() {
    this.triggerMergeIfNeeded();
    if (this.stream) {
      this.stream.getTracks().forEach((t) => t.stop());
      this.stream = null;
    }
  }

  async checkPermissions() {
    try {
      const s = await navigator.mediaDevices.getUserMedia({
        video: true,
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true,
          channelCount: 1,
          sampleRate: 48000,
        },
      });
      this.stream = s;
      this.micStream = new MediaStream(s.getAudioTracks());
      this.permissionsGranted = true;
      this.permissionError = null;
      // const micStream = new MediaStream(s.getAudioTracks());
      this.initMicAudioDetection(this.micStream!);
      setTimeout(() => this.attachStreamToVideo().catch(() => { }), 0);
    } catch (err) {
      this.permissionsGranted = false;
      this.permissionError =
        "Please allow access to your camera and microphone to proceed.";
    }
  }

  private async initMicAudioDetection(stream: MediaStream): Promise<void> {
    // this.audioContext = new AudioContext();
    this.audioContext = new (
      window.AudioContext || (window as any).webkitAudioContext
    )();
    // Ensure the context is active
    if (this.audioContext.state === "suspended") {
      await this.audioContext.resume();
    }
    this.micSource = this.audioContext.createMediaStreamSource(stream);
    this.analyser = this.audioContext.createAnalyser();
    this.analyser.fftSize = 2048; //512 earlier
    this.micSource.connect(this.analyser);
    const buffer = new Uint8Array(this.analyser.frequencyBinCount);
    const detect = () => {
      if (!this.candidateCanSpeak) {
        requestAnimationFrame(detect);
        return;
      }
      this.analyser.getByteTimeDomainData(buffer);
      // Detect sound deviation from silence (128)
      // const hasAudio = buffer.some((v) => Math.abs(v - 128) > 6);
      // if (hasAudio) {
      //   this.candidateSpoke = true;
      //   this.answeredQuestionMap[this.currentQuestionIndex] = 1;
      // }

      // Calculate Root Mean Square (RMS) for better voice detection than simple deviation
      let sum = 0;
      for (let i = 0; i < buffer.length; i++) {
        const val = (buffer[i] - 128) / 128;
        sum += val * val;
      }
      const rms = Math.sqrt(sum / buffer.length);

      // Threshold: 0.01 is a whisper, 0.05 is normal speaking
      if (rms > 0.02) {
        this.candidateSpoke = true;
        this.answeredQuestionMap[this.currentQuestionIndex] = 1;
      }
      requestAnimationFrame(detect);
    };
    detect();
  }

  private async attachStreamToVideo(): Promise<void> {
    if (!this.videoElementRef || !this.videoElementRef.nativeElement) return;
    const el = this.videoElementRef.nativeElement as HTMLVideoElement;
    if (!this.stream) return;

    el.srcObject = this.stream;
    el.muted = true;
    el.playsInline = true;

    try {
      await el.play();
    } catch (e) { }

    if (el.readyState >= HTMLMediaElement.HAVE_ENOUGH_DATA) return;

    await new Promise<void>((resolve) => {
      const onPlay = () => {
        el.removeEventListener("playing", onPlay);
        resolve();
      };
      el.addEventListener("playing", onPlay);

      setTimeout(() => {
        el.removeEventListener("playing", onPlay);
        resolve();
      }, 1500);
    });
  }

  async startInterview() {
    if (!this.permissionsGranted) return;
    this.started = true;
    this.paused = false;
    this.currentQuestionIndex = 0;
    this.askedQuestions = [];

    this.http
      .get<any>(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/next-question`,
      )
      .subscribe({
        next: async (result) => {
          if (result && result.question) {
            this.currentQuestionIndex = result.seqId;
            this.currentQuestion = { text: result.question };
            this.askedQuestions.push({
              text: result.question,
              section: 1,
              question: result.index + 1,
            });

            await this.prepareAndStartRecording();
            await this.speakQuestion(result.question);
          } else if (result && result.signal === "END") {
            await this.stopRecording();
            this.router.navigate(["/view-interview", this.code]);
          }
        },
        error: () => {
          this.error = "Could not fetch first question.";
        },
      });
  }

  speakQuestion(text: string): Promise<void> {
    this.candidateCanSpeak = false;
    this.candidateSpoke = false;
    return new Promise((resolve) => {
      speechSynthesis.cancel();
      const cleanedText = text.replace(/`/g, "");
      const utterance = new SpeechSynthesisUtterance(cleanedText);

      utterance.voice = this.selectedVoice!;
      utterance.lang = "en-US";
      utterance.rate = 0.9; // slower = more human
      utterance.pitch = 0.95; // slightly deeper
      utterance.volume = 1;
      utterance.lang = "en-US";
      utterance.rate = 0.9;
      utterance.pitch = 0.95;

      //Start Avatar Video when speech begins
      utterance.onstart = () => {
        if (this.avatarVideo) {
          this.avatarVideo.nativeElement
            .play()
            .catch((err) => console.warn("Video play blocked", err));
        }
      };

      utterance.onend = () => {
        if (this.avatarVideo) {
          this.avatarVideo.nativeElement.pause();
          this.avatarVideo.nativeElement.currentTime = 0; // Reset to start frame
        }
        // Tiny buffer to let the Echo Canceller settle after AI stops
        setTimeout(() => {
          this.candidateCanSpeak = true;
          resolve();
        }, 300);
      };

      // ❌ Safety stop on error
      utterance.onerror = () => {
        if (this.avatarVideo) this.avatarVideo.nativeElement.pause();
        this.candidateCanSpeak = true;
        resolve();
      };

      speechSynthesis.speak(utterance);
    });
  }

  async ongoingInterviewAction(action: string) {
    await new Promise((r) => setTimeout(r, 300));
    const hasAnswered = this.hasUserAnsweredCurrentQuestion();
    await this.stopRecording();
    this.hardStopAllMedia();
    // allow analyser to settle

    // const hasAnswered = this.hasUserAnsweredCurrentQuestion();
    if (hasAnswered) {
      action === "next_question" ? this.nextQuestion() : this.pauseInterview();
    } else {
      const dialogData =
        action === "next_question"
          ? nextQuestionConfirmationDialogue
          : pauseInteviewConfirmationDialogue;

      this.confirmDialogue(dialogData);
    }
  }

  private hasUserAnsweredCurrentQuestion(): boolean {
    return this.answeredQuestionMap[this.currentQuestionIndex] === 1;
  }

  confirmDialogue(data: any): void {
    const dialogRef = this.dialog.open(CommonDialogComponent, {
      width: "450px",
      disableClose: true,
      data: {
        title: data.title,
        message: data.message,
        showActions: true,
        confirmText: data.confirmText,
        cancelText: data.cancelText,
      },
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (confirmed === true) {
        data.modalName === "confirm_next"
          ? this.nextQuestion()
          : this.pauseInterview();
      } else {
        this.enableCandidateAnswering();
        await this.prepareAndStartRecording();
        // await this.startRecording();
      }
    });
  }

  private enableCandidateAnswering(): void {
    this.candidateSpoke = false;
    this.candidateCanSpeak = true;
  }

  private hardStopAllMedia(): void {
    // Stop MediaRecorder
    if (this.mediaRecorder) {
      if (this.mediaRecorder.state !== "inactive") {
        this.mediaRecorder.stop();
      }
      this.mediaRecorder = null;
    }

    // Stop camera + mic
    if (this.stream) {
      this.stream.getTracks().forEach((t) => {
        console.log("Stopping track:", t.kind);
        t.stop();
      });
      this.stream = null;
    }

    if (this.micStream) {
      this.micStream.getTracks().forEach((t) => t.stop());
      this.micStream = null;
    }

    if (this.canvasStream) {
      this.canvasStream.getTracks().forEach((t) => t.stop());
      this.canvasStream = null;
    }

    // Detach video
    if (this.videoElementRef?.nativeElement) {
      const v = this.videoElementRef.nativeElement;
      v.pause();
      v.srcObject = null;
      v.load();
    }
  }

  async nextQuestion() {
    const mergePromise = this.triggerMergeIfNeededV2();
    if (mergePromise) {
      try {
        await mergePromise;
      } catch (e) { }
    }

    this.http
      .get<any>(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/next-question`,
      )
      .subscribe({
        next: async (result) => {
          if (result && result.signal === "END") {
            this.currentQuestion = null;
            this.started = false;
            this.paused = false;
            await this.stopRecording();
            this.hardStopAllMedia();
            console.log(
              "Active devices:",
              this.stream?.getTracks().map((t) => ({
                kind: t.kind,
                state: t.readyState,
              })),
            );
            this.router.navigate(["/view-interview", this.code]);
          } else if (result && result.question) {
            this.currentQuestionIndex = result.seqId;
            this.currentQuestion = { text: result.question };
            this.askedQuestions.push({
              text: result.question,
              section: 1,
              question: result.index + 1,
            });
            await this.prepareAndStartRecording();
            await this.speakQuestion(result.question);
          }
        },
        error: () => {
          this.error = "Could not fetch next question.";
        },
      });
  }

  async pauseInterview() {
    this.paused = true;
    const questionNumber = this.currentQuestionIndex;
    this.http
      .post(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/questions/${questionNumber}/merge-chunks`,
        null,
        { responseType: "text" },
      )
      .subscribe();
    this.http
      .get<
        any[]
      >(`${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/questions/progress`)
      .subscribe(
        (progress) => {
          this.askedQuestions = progress || [];
        },
        () => {
          this.askedQuestions = [];
        },
      );
    this.pauseCountdown = 3;
    const countdownInterval = setInterval(() => {
      if (this.pauseCountdown !== null) {
        this.pauseCountdown--;
        if (this.pauseCountdown === 0) {
          clearInterval(countdownInterval);
          this.started = false;
          this.pauseCountdown = null;
        }
      }
    }, 1000);
  }

  async resumeInterview() {
    this.started = true;
    this.paused = false;
    this.http
      .get<any>(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/next-question`,
      )
      .subscribe({
        next: async (result) => {
          if (result && result.question) {
            this.currentQuestionIndex = result.seqId;
            this.currentQuestion = { text: result.question };
            this.askedQuestions.push({
              text: result.question,
              section: 1,
              question: result.index + 1,
            });
            await this.prepareAndStartRecording();
            await this.speakQuestion(result.question);
          } else if (result && result.signal === "END") {
            this.currentQuestion = null;
            await this.stopRecording();
            this.router.navigate(["/view-interview", this.code]);
          }
        },
        error: () => {
          this.error = "Could not fetch first question.";
        },
      });
  }

  private triggerMergeIfNeeded() {
    if (this.mergeTriggered || !this.code || !this.currentQuestion) return;
    const questionNumber = this.currentQuestionIndex;
    this.http
      .post(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/questions/${questionNumber}/merge-chunks`,
        null,
      )
      .subscribe();
    this.mergeTriggered = true;
  }

  async startRecording() {
    this.mergeTriggered = false; // Reset for new recording/question

    if (!this.permissionsGranted) {
      this.permissionError = "Permissions not granted.";
      return;
    }
    if (!this.stream) {
      console.error("Camera or avatar not ready");
      return;
    }
    const micStream = this.micStream;
    if (!micStream) {
      console.error("Streams not ready");
      return;
    }
    let canvasStream = this.captureCanvasStream();
    const finalStream = new MediaStream([
      ...canvasStream.getVideoTracks(),
      ...micStream.getAudioTracks(),
    ]);

    const options: MediaRecorderOptions = {
      mimeType: "video/webm;codecs=vp8,opus",
      audioBitsPerSecond: 192000,
      videoBitsPerSecond: 1500000,
    };
    this.recordedChunks = [];
    this.chunkIndex = 0;

    this.mediaRecorder = new MediaRecorder(finalStream, options);
    this.mediaRecorder.ondataavailable = (event) => {
      if (event.data && event.data.size > 0) {
        const localIndex = this.chunkIndex++;
        this.recordedChunks.push(event.data);
        const micStream = new MediaStream(finalStream.getAudioTracks());
        this.initMicAudioDetection(micStream);
        // Upload chunk to backend
        const formData = new FormData();
        formData.append("chunkIndex", localIndex.toString());
        formData.append("file", event.data, `chunk-${localIndex}.webm`);
        this.http
          .post(
            `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/questions/${this.currentQuestionIndex}/chunks`,
            formData,
            {
              responseType: "text",
              context: new HttpContext().set(SKIP_LOADER, true),
            },
          )
          .subscribe({
            next: () => { },
            error: (err) =>
              console.error("Chunk upload failed", localIndex, err),
          });
      }
    };
    this.mediaRecorder.onerror = (e) => console.error("MediaRecorder error", e);
    this.mediaRecorder.start(2000);
  }

  async stopRecording() {
    try {
      if (!this.mediaRecorder) {
        console.warn("stopRecording called but mediaRecorder is missing");
        return;
      }

      if (
        this.mediaRecorder.state !== "recording" &&
        this.mediaRecorder.state !== "paused"
      ) {
        console.warn(
          "mediaRecorder not recording/paused, current state:",
          this.mediaRecorder.state,
        );
      }

      // Wrap stop/wait logic in a Promise so we only assemble the final blob after onstop and final chunks
      await new Promise<void>((resolve, reject) => {
        let stopped = false;
        const onStop = () => {
          if (stopped) return;
          stopped = true;
          try {
            this.mediaRecorder?.removeEventListener("stop", onStop);
          } catch (e) { }
          resolve();
        };

        try {
          this.mediaRecorder?.addEventListener("stop", onStop);
          // Request any pending data immediately before stopping
          try {
            this.mediaRecorder?.requestData();
          } catch (e) { }
          this.mediaRecorder?.stop();
        } catch (err) {
          reject(err);
        }

        setTimeout(() => resolve(), 2000);
      });

      const finalBlob = new Blob(this.recordedChunks, {
        type: this.mediaRecorder?.mimeType || "video/webm",
      });

      // stop tracks (but keep stream null only if we want to force re-acquire on next start)
      // In this implementation we stop tracks to free devices when not recording. If you want instant resume
      // you may keep the tracks alive and only stop them on ngOnDestroy.
      if (this.stream) {
        this.stream.getTracks().forEach((t) => t.stop());
        this.stream = null;
      }

      const url = URL.createObjectURL(finalBlob);
      this.answeredQuestionMap[this.currentQuestionIndex] = this.candidateSpoke
        ? 1
        : 0;

      return { blob: finalBlob, url };
    } catch (err) {
      console.error("stopRecording error", err);
      throw err;
    }
  }
  private triggerMergeIfNeededV2(): Promise<any> | null {
    if (this.mergeTriggered || !this.code || !this.currentQuestion) return null;
    const questionNumber = this.currentQuestionIndex;
    this.mergeTriggered = true;
    return this.http
      .post(
        `${environment.apiMockinterviewBaseURL}/interview/code/${this.code}/questions/${questionNumber}/merge-chunks`,
        null,
      )
      .toPromise();
  }
  async restartRecording() {
    await this.stopRecording();
    this.triggerMergeIfNeeded();
    await this.checkPermissions();
    await this.prepareAndStartRecording();
  }

  animatedScore: number = 0; // score that animates from 0 → final

  animateScore() {
    if (!this.summaryData?.modelScore) return;

    const finalScore = this.summaryData.modelScore;
    this.capturedScore = parseInt(finalScore.split("/")[0], 10) || 0;

    this.animatedScore = 0;

    const duration = 9000; // 9s animation (slower sweep)
    const start = performance.now();

    // 🔹 Alternative easing functions
    const easeInOutCubic = (t: number) =>
      t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

    const step = (timestamp: number) => {
      const progress = Math.min((timestamp - start) / duration, 1); // 0 → 1
      const eased = easeInOutCubic(progress); // 👈 swap with easeOutExpo if you prefer
      this.animatedScore = Math.round(this.capturedScore * eased);

      if (progress < 1) {
        requestAnimationFrame(step);
      }
    };

    requestAnimationFrame(step);
  }
  get scoreLabel(): string {
    const scoreValue = Number(
      this.summaryData?.modelScore?.split("/")[0]?.trim() || 0,
    );
    const range = this.scoreRanges.find(
      (r) => scoreValue >= r.min && scoreValue <= r.max,
    );
    return range ? range.label : "N/A";
  }

  onToggle() {
    if (this.isDialogOpen) {
      this.dialog.closeAll();
      this.isDialogOpen = false;
    } else {
      const dialogData: DialogData = {
        title: "Comments",
        message: this.summaryData?.modelComments,
        showActions: false,
      };

      const dialogRef = this.dialog.open(CommonDialogComponent, {
        width: "700px",
        maxWidth: "700px",
        data: dialogData,
      });

      this.isDialogOpen = true;

      dialogRef.afterClosed().subscribe(() => {
        this.isDialogOpen = false;
      });
    }
  }

  getDisplayStatus(status?: string): string {
    if (!status) return "N/A";

    const statusMap: Record<string, string> = {
      ASSIGNED: "Assigned",
      PENDING: "Pending",
      processing: "In Progress",
      progressing: "In Progress",
      ended: "Completed",
    };

    return statusMap[status] ?? status;
  }

  loadVoices() {
    this.voices = speechSynthesis.getVoices();

    // Prefer natural English voices
    this.selectedVoice =
      this.voices.find((v) => v.name.includes("Google US English")) ||
      this.voices.find((v) => v.name.includes("Microsoft")) ||
      this.voices.find((v) => v.lang === "en-US");
  }

  async initUserCamera() {
    try {
      this.stream = await navigator.mediaDevices.getUserMedia({
        video: true,
        audio: true,
      });

      this.videoElementRef.nativeElement.srcObject = this.stream;
      await this.videoElementRef.nativeElement.play();
      //   await this.video.nativeElement.play();
    } catch (err) {
      console.error("Camera access failed", err);
    }
  }

  startCanvasDrawing() {
    const canvas = this.mixCanvas.nativeElement;
    const ctx = canvas.getContext("2d")!;

    canvas.width = 1280;
    canvas.height = 720;

    const drawFrame = () => {
      if (!this.started) {
        requestAnimationFrame(drawFrame);
        return;
      }
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      const video = this.videoElementRef.nativeElement;
      if (video && video.readyState >= 2) {
        // Ensure video has data
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      }

      requestAnimationFrame(drawFrame);
    };

    drawFrame();
  }

  captureCanvasStream() {
    const canvasStream = this.mixCanvas.nativeElement.captureStream(20);
    return canvasStream;
  }

  async prepareAndStartRecording() {
    try {
      if (!this.stream) {
        await this.checkPermissions(); // init ONCE
      }
      await this.initUserCamera();
      this.startCanvasDrawing();
      await this.safePlayVideo(this.videoElementRef.nativeElement);
      // await this.safePlayVideo(this.video.nativeElement);

      await this.startRecording();
    } catch (err) {
      console.error("Failed to prepare and start recording:", err);
    }
  }

  private async safePlayVideo(videoEl: HTMLVideoElement) {
    if (!videoEl) return;

    videoEl.muted = true; // 🔴 REQUIRED
    videoEl.volume = 0; // extra safety

    videoEl.autoplay = true;
    try {
    } catch (err) {
      console.warn("Video play blocked, muting and retrying:", err);
      videoEl.muted = true; // allow autoplay with muted
      await videoEl.play().catch(() => { });
    }
  }

  get isInterviewDisabled(): boolean {
    return !this.isHeadphoneConfirmed || !this.isTabPolicyConfirmed;
  }
}
