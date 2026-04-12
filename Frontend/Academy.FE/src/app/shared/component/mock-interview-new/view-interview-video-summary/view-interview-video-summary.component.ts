import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { CommonModule, Location } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { Profile, ProfileService } from "../../../../services/profile.service";
import {
  Skills,
  SkillsServiceService,
} from "../../../../services/skills.service";
import { environment } from "../../../../../environments/environment";
import {
  DomSanitizer,
  SafeHtml,
} from "@angular/platform-browser";
import { InterviewsService } from "../../../../services/interviews.service";
import { SpeedoMeterComponent } from "../speedo-meter/speedo-meter.component";
import { startWith, switchMap, takeWhile, interval, map } from "rxjs";
import { SkillsRatingComponent } from "../skills-rating/skills-rating.component";
import { VideoPlayerComponent } from "./video-player.component";

@Component({
  selector: "app-view-interview-video-summary",
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    SpeedoMeterComponent,
    SkillsRatingComponent,
    VideoPlayerComponent
  ],
  templateUrl: "./view-interview-video-summary.component.html",
  styleUrl: "./view-interview-video-summary.component.css",
})
export class ViewInterviewVideoSummaryComponent {
  code: string = "";
  interview: any = null;
  profile: Profile | null = null;
  skills: Skills[] = [];
  loading = true;
  error: string | null = null;
  showSummary = false;
  summaryData: any;
  safeUrl!: string;
  isDisabled = true;
  interviewDetails: any = null;
  showTranscript = false;
  capturedScore: any;
  pollingFinished = false;
  summary: string = '';
  strengths: string = '';
  improvements: string = '';
  skillRatingData: any;
  isLoading = true;
  private sectionMap: Record<string, 'summary' | 'strengths' | 'improvements'> = {
    'Summary': 'summary',
    'Strengths': 'strengths',
    'Improvement Areas': 'improvements'
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private profileService: ProfileService,
    private skillsService: SkillsServiceService,
    private sanitizer: DomSanitizer,
    private interviewService: InterviewsService,
    private location: Location,
  ) { }

  ngOnInit(): void {
    this.code = this.route.snapshot.paramMap.get("code") || "";

    if (!this.isValidCode(this.code)) {
      this.setError("Invalid evaluation code.");
      return;
    }

    this.loadInterview(this.code);
  }

  private isValidCode(code: string): boolean {
    return /^\d{3}-\d{3}-\d{3}-\d{3}$/.test(code);
  }

  private setError(message: string): void {
    this.error = message;
    this.loading = false;
  }

  private loadInterview(code: string): void {
    this.http
      .get<any>(`${environment.apiMockinterviewBaseURL}/interview/code/${code}`)
      .subscribe({
        next: (data) => this.handleInterview(data),
        error: () => this.setError("Evaluation not found."),
      });
  }

  private handleInterview(data: any): void {
    this.interview = data;
    this.handleSummary(data);

    if (!data?.profile) {
      this.loading = false;
      return;
    }

    this.loadProfile(data.profile);
  }

  makePreviewLink(driveUrl: string) {
    const fileId = driveUrl.split("/d/")[1]?.split("/")[0];
    return `https://drive.google.com/file/d/${fileId}/preview`;
  }

  private handleSummary(data: any): void {
    if (data?.status !== "Completed") return;

    this.showSummary = true;

    this.pollInterviewDetails(this.code);
  }

  public fetchSummaryDetails(id: string) {
    this.interviewService.getInterviewSummary(id).subscribe({
      next: (summary) => {
        this.summaryData = summary;
        if (this.summaryData.driveProcessingStatus && this.summaryData.driveProcessingStatus === "COMPLETE") {
          this.isDisabled = false;
        }
        this.extractUsingDOM(this.summaryData.modelComments);
        this.interviewSkillScore();
        this.animateScore();

        // ✅ NEW: Fetch signed URL from backend
        this.fetchSignedVideoUrl();
      },
      error: (err) => {
        console.error("Failed to load evaluation summary", err);
      },
    });
  }
  fetchSignedVideoUrl(): void {
    if (!this.interview?.interviewCode) return;
    this.interviewService.fetchSignedVideoUrl(this.interview?.interviewCode).subscribe({
      next: (signedPath: string) => {
        this.safeUrl = environment.apiMockinterviewBaseURL + signedPath;
      },
      error: (err) => {
        console.error("Failed to fetch signed video URL", err);
      }
    });
  }

  extractUsingDOM(text: string) {
    if (!text) return;
    const doc = new DOMParser().parseFromString(text, 'text/html');
    const boldTags = Array.from(doc.querySelectorAll('b'));
    boldTags.forEach(tag => {
      const key = this.sectionMap[tag.textContent?.trim() || ''];
      if (!key) return;
      const content: string[] = [];
      let next = tag.nextSibling;

      while (next && next.nodeName !== 'B') {
        content.push(next.textContent || '');
        next = next.nextSibling;
      }

      this[key] = content.join('').trim();
    });
  }

  private loadProfile(profile: any): void {
    this.profileService.getById(profile?.profileId).subscribe({
      next: (profile) => this.handleProfile(profile),
      error: () => (this.loading = false),
    });
  }

  private handleProfile(profile: Profile): void {
    this.profile = profile;
    const skillIds =
      profile?.skillsAndSections?.map(s => s.skillId) || [];

    if (!skillIds?.length) {
      this.loading = false;
      return;
    }

    this.loadSkills(skillIds);
  }

  private loadSkills(skillIds: number[]): void {
    this.skillsService.getAll().subscribe({
      next: (allSkills) => {
        const validIds = skillIds.filter(
          (id): id is number => typeof id === "number"
        );

        this.skills = allSkills.filter(
          (s) => typeof s.id === "number" && validIds.includes(s.id)
        );

        this.loading = false;
      },
      error: () => (this.loading = false),
    });
  }

  public pollInterviewDetails(id: string) {
    interval(15000)
      .pipe(
        startWith(0),
        switchMap(() =>
          this.interviewService.fetchInterviewDetailById(id)
        ),
        takeWhile((details: any) => {
          if (!details) return true;

          this.interviewDetails = structuredClone(details);

          const questions = this.interviewDetails.questions || [];
          const lastQuestion = questions[questions.length - 1];

          const isLastAnalysisReady =
            lastQuestion?.analysis !== null;

          // keep polling while condition is NOT met
          return !(isLastAnalysisReady);
        }, true)
      )
      .subscribe({
        next: (details) => {
          // optional: handle each polling update
        },
        complete: () => {
          // Polling finished
          this.pollingFinished = true; // ✅ Show UI now
          this.fetchSummaryDetails(id);
        },
        error: (err) => {
          console.error(err);
          this.pollingFinished = true; // fail-safe
        }
      });
  }


  toggleTranscript() {
    this.showTranscript = !this.showTranscript;
  }

  animatedScore: number = 0; // score that animates from 0 → final

  animateScore() {
    if (!this.summaryData?.modelScore) return;

    const finalScore = this.summaryData.modelScore;
    this.capturedScore = parseInt(finalScore.split("/")[0], 10) || 0;

    this.animatedScore = 0;

    const duration = 4000; // 9s animation (slower sweep)
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

  getSanitizedComment(comment: string | null): SafeHtml | string {
    if (!comment) {
      return "N/A";
    }

    return this.sanitizer.bypassSecurityTrustHtml(comment);
  }

  proceedInterview() {
    this.router.navigate(["/interview", this.code]);
  }

  private interviewSkillScore(): void {
    this.isLoading = true;
    this.interviewService.getSkillWiseScore(this.code)
      .pipe(
        map((res: any) =>
          res.skillEvaluations.map((item: any) => ({
            skillName: item.skillName,
            skillRatingOutOfFive: item.skillRatingOutOfFive,
            percentage: (item.skillRatingOutOfFive / 5) * 100,
            userRatingOutOfFive: item.userRatingOutOfFive ? item.userRatingOutOfFive : 0,
            userRatingPercentage: (item.userRatingOutOfFive / 5) * 100,
          }))
        )
      )
      .subscribe({
        next: response => {
          this.skillRatingData = response;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  profileName(profile: any) {
    return [
      profile?.clientName,
      profile?.position,
      profile?.primarySkillName
    ]
      .filter(Boolean)
      .join('_');
  }

  evaluationType(profile: any) {
    return profile?.fitmentTypeName ?? '';
  }
  goBack(): void {
    this.location.back();
  }
}
