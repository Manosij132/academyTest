import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { VideoRecorderService } from '@services/video-recorder.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AcademyHttpService } from '@services/academy-http.service';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router'; 
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

interface InterviewQuestion {
  questionId: number;
  questionText: string;
}


@Component({
  selector: 'app-mock-interview',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    MatButtonModule,
    MatTooltipModule,
    MatCardModule,
  ],
  templateUrl: './mock-interview.component.html',
  styleUrls: ['./mock-interview.component.css'],
})
export class MockInterviewComponent implements AfterViewInit, OnInit {
  questions: InterviewQuestion[] = [];
  currentIndex = 0;
  currentQuestion: InterviewQuestion | null = null;
  isInterviewComplete: boolean = false;
  interviewTime = 5;
  unit = 'Second'; // Minute or Second
   interviewId!: any; 
   isWaitingToStart = false;
   isInterviewInterrupted: boolean = false;
   isSubmitDisabled = false;


 

  @ViewChild('videoElement', { static: true })
  videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('snackBarTemplate', { static: true })
  snackBarTemplate!: TemplateRef<any>;
  private stream!: MediaStream;

  isRecording = false;
  timer = '00:00:00';
  interval: any;
  private _snackBar = inject(MatSnackBar);

  isMediaPermissionGranted = false;
  isInstructionsPageVisible = true;
  isErrorOccurred = false;
  isInterviewTimerActive = false; 
  private isApiCallInProgress = false;

  constructor(
    private videoRecorderService: VideoRecorderService,
    private interviewDetailsService: AcademyHttpService,
    private route: ActivatedRoute,
    private router:Router,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.interviewId = this.route.snapshot.paramMap.get('id')!;
    const interrupted = sessionStorage.getItem('interviewInterrupted');
    const interviewComplete = sessionStorage.getItem('isInterviewComplete');
    
    if (interviewComplete === 'true') {
      this.isInterviewComplete = true;
      this.isInterviewInterrupted = false;
    } else if (interrupted === 'true') {
      this.isInterviewInterrupted = true;
      this.isInterviewComplete = true; 
      // sessionStorage.removeItem('interviewInterrupted');
      sessionStorage.removeItem('lastQuestionIndex');
    } else {
      this.fetchQuestions();
    }
    window.addEventListener('beforeunload', this.handlePageRefresh);
  }
  


  ngOnDestroy() {
    if (typeof window !== 'undefined') {
      window.removeEventListener('beforeunload', this.handlePageRefresh);
    }
  }
  
  


  redirectToContactAdmin() {
    this.router.navigate(['/list']);  
  }
 
  handlePageRefresh = () => {
    if (this.isInterviewComplete || this.isInterviewInterrupted) {
      //console.log('[🛑] Interview already completed. No merge on refresh.');
      return;
    }
  
    const currentQuestionId = this.questions[this.currentIndex]?.questionId;
    const isLastQuestion = true; 
  
    //console.log('[🔁] Page refresh — treating this as interruption');
  
    this.videoRecorderService.stopChunkedRecording().then(({ answerRequest, totalChunks }) => {
      answerRequest.last = isLastQuestion;
  
      this.videoRecorderService.mergeChunks(answerRequest, totalChunks,this.currentIndex).subscribe(() => {
        sessionStorage.setItem('interviewInterrupted', 'true');
        sessionStorage.setItem('lastQuestionIndex', this.currentIndex.toString());
      });
    });
  };
  


  fetchQuestions() {
    this.videoRecorderService
      .getInterviewQuestionsByInterviewId(this.interviewId)  
      .subscribe(
        (response: InterviewQuestion[]) => {
          if (response && response.length > 0) {
            this.questions = response;
            this.currentQuestion = this.questions[this.currentIndex];
          } else {
            console.error('No questions fetched from the API');
            this.isErrorOccurred = true;
          }
        },
        (error) => {
          console.error('Error fetching questions:', error);
          this.isErrorOccurred = true;
        }
      );
  }

  async ngAfterViewInit() {
    if (this.isInterviewComplete || this.isInterviewInterrupted || this.isErrorOccurred) {
      console.warn('[🚫] Skipping media permissions: Interview completed or interrupted.');
      return;
    }
  
    if (!this.isMediaPermissionGranted) {
      // await this.requestMediaPermissions();
    }
  }
  
  async requestMediaPermissions() {
    try {
      this.stream = await navigator.mediaDevices.getUserMedia({
        video: true,
        audio: true,
      });
      this.isMediaPermissionGranted = true;
      this.isInstructionsPageVisible = false;
      this.videoElement.nativeElement.srcObject = this.stream;
      this.videoElement.nativeElement.muted = true; 


      this.startRecordingAndTimer();
    } catch (error) {
      console.error('Error accessing media devices:', error);
      this.toastr.error(
        'Camera and microphone access is required to proceed. Please grant permissions and refresh the page.',
        'Error',
        {
          timeOut: 5000,
          closeButton: true,  
        }
      );
    }
  }

  private startRecordingAndTimer() {
    if (!this.isInterviewTimerActive) {
      this.isInterviewTimerActive = true;
      this.startTimer();
    }
  
    this.isRecording = true;
    this.videoRecorderService.startChunkedRecording(
      this.stream,
      this.interviewId,
      this.questions[this.currentIndex].questionId
    );
    
  }
  

  

  startTimer() {
    let elapsedTimeInSeconds = 0;

    this.interval = setInterval(() => {
      elapsedTimeInSeconds++;
      const hours = Math.floor(elapsedTimeInSeconds / 3600);
      const minutes = Math.floor((elapsedTimeInSeconds % 3600) / 60);
      const seconds = elapsedTimeInSeconds % 60;
      this.timer = `${this.formatTime(hours)}:${this.formatTime(
        minutes
      )}:${this.formatTime(seconds)}`;
    }, 1000);
  }

  formatTime(time: number): string {
    return time < 10 ? `0${time}` : `${time}`;
  }

  stopTimer() {
    if (this.isInterviewTimerActive) { 
      clearInterval(this.interval);
      this.timer = '00:00:00';
    }
  }

  formatTimeForDisplay(totalSeconds: number): string {
    return totalSeconds.toFixed(2);
  }


  onNext() {
    if (!this.isRecording) return;
    const currentQuestionId = this.questions[this.currentIndex]?.questionId;
  
    const isLastQuestion = this.currentIndex === this.questions.length - 1;
  
    // Stop recording and merge chunks asynchronously without waiting for completion
    this.videoRecorderService.stopChunkedRecording().then(({ answerRequest, totalChunks }) => {
      answerRequest.last = isLastQuestion;
  
      // Merge chunks in the background without blocking UI
      this.videoRecorderService.mergeChunks(answerRequest, totalChunks,this.currentIndex).subscribe({
        next: () => {
          //console.log('Chunks merged successfully');
        },
        error: (err) => {
          console.error('Error merging chunks:', err);
        },
      });
  
      // Move to the next question immediately, without waiting for the upload
      this.moveToNextQuestion();
    });
  }
  
  private moveToNextQuestion() {
    this.currentIndex++;
    if (this.currentIndex === this.questions.length - 1) {
      this.isSubmitDisabled = true;
  
      setTimeout(() => {
        this.isSubmitDisabled = false;
      }, 5000);
    }
    if (this.currentIndex < this.questions.length) {
      this.currentQuestion = this.questions[this.currentIndex];
      this.startRecordingAndTimer();
    } else {
      this.currentQuestion = null;
      this.isInterviewComplete = true;
      this.stopTimer();
    }
  }
  stopMediaStream() {
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.videoElement.nativeElement.srcObject=null;
    }
  }
  
  
  
  submitInterview() {
    if (!this.isInterviewComplete && this.isRecording) {
      const isLastQuestion = true;
      const currentQuestionId = this.questions[this.currentIndex]?.questionId;
      
  
      // Stop recording and merge chunks asynchronously without waiting for completion
      this.videoRecorderService.stopChunkedRecording().then(({ answerRequest, totalChunks }) => {
        answerRequest.last = isLastQuestion;
  
        this.videoRecorderService.mergeChunks(answerRequest, totalChunks,this.currentIndex).subscribe();

  
        // Mark the interview as complete immediately
        this.isInterviewComplete = true;
        this.stopTimer();
        this.stopMediaStream();
        sessionStorage.setItem('isInterviewComplete', 'true');
      });
    }
  }
  
  
  
  


  saveFile(blob: Blob, questionId: number) {
   
    const a = document.createElement('a');
    const url = URL.createObjectURL(blob);
    a.href = url;
    a.download = `interview_${this.interviewId}_question_${questionId}.webm`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
  
}




