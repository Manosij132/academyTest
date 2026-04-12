import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class VideoRecorderService {
  private mockApiUrl :string;
  constructor(private http: HttpClient, private snackBar: MatSnackBar,private toastr: ToastrService) {
    this.mockApiUrl=environment.apiMockinterviewBaseURL
  }

  private mediaRecorder!: MediaRecorder;
private chunkIndex = 0;
private stream!: MediaStream;
private currentAnswerRequest: any;
 

startChunkedRecording(
  stream: MediaStream,
  interviewId: number,
  questionId: number
) {
  // console.log('[🎥] Initializing MediaRecorder for question:', questionId);

  this.chunkIndex = 0;
  this.stream = stream;

  const answerRequest = {
    interviewId,
    questionId,
    last: false,
  };

  this.currentAnswerRequest = answerRequest;

  this.mediaRecorder = new MediaRecorder(stream, {
    mimeType: 'video/webm; codecs=vp8,opus',
  });

  this.mediaRecorder.ondataavailable = (event: BlobEvent) => {

    if (event.data.size > 0) {
      const formData = new FormData();
      formData.append(
        'chunk',
        event.data,
        `chunk-${this.chunkIndex}.webm`
      );
      formData.append(
        'answer',
        new Blob([JSON.stringify(answerRequest)], { type: 'application/json' })
      );
      formData.append('chunkIndex', this.chunkIndex.toString());

   

      this.http.post(`${this.mockApiUrl}/mock-interviews/chunk`, formData, {
        responseType: 'text' as 'json',
      }).subscribe({
        next: () => {
       
        },
        error: (err) => {
      
        },
      });

      this.chunkIndex++;
    }
  };

 
  this.mediaRecorder.start(2000);
}


stopChunkedRecording(): Promise<any> {
  return new Promise((resolve) => {
    this.mediaRecorder.onstop = () => {
      resolve({
        answerRequest: this.currentAnswerRequest,
        totalChunks: this.chunkIndex,
      });
    };
    this.mediaRecorder.stop();
    this.mediaRecorder = undefined!;

  });
}


mergeChunks(answerRequest: any, totalChunks: number, questionIndex: number): Observable<any> {
  // console.log(answerRequest,totalChunks,questionIndex)
  const formData = new FormData();
  formData.append(
    'answer',
    new Blob([JSON.stringify(answerRequest)], {
      type: 'application/json',
    })
  );
  formData.append('totalChunks', totalChunks.toString());
  formData.append('questionIndex', questionIndex.toString()); 

  return this.http.post(
    `${this.mockApiUrl}/mock-interviews/merge-chunk`,
    formData,
    { responseType: 'text' as 'json' }
  );
}




  getInterviewQuestionsByInterviewId(interviewId: string): Observable<any> {
    const url = `${this.mockApiUrl}/mock-interviews/questions?interviewId=${interviewId}`;
    return this.http.get(url).pipe(
      tap((response) => {
        // //console.log('Questions fetched: ', response);
      }),
      catchError((error) => {
        console.error('Error fetching questions: ', error);
        this.toastr.error(
          'Failed to load evaluation questions. Please try again later.',
          'Error',
          {
            timeOut: 3000,
            closeButton: true,  
          }
        );
        return of(error);
      })
    );
  }
  
  
  validateInterviewId(interviewId: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${this.mockApiUrl}/mock-interviews/interview/validate/${interviewId}`
    ).pipe(
      tap((response) => {
        // //console.log('Interview ID validation response: ', response);
      }),
      catchError((error) => {
        console.error('Error validating interview ID: ', error);
        this.toastr.error(
          'Failed to validate evaluation ID. Please try again later.',
          'Error',
          {
            timeOut: 3000,
            closeButton: true,   
          }
        );
        return of(false);
      })
    );
  }
  
}