import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AcademyHttpService } from '../../../services/academy-http.service';
import {
  TrainingStatus,
  CompletedTrainingStatus,
} from '../../../shared/constants/app.constants';
import { ChangeStatusRequest } from '../../dto/change-status-request';
import { DataTransferService } from '../../../services/data-transfer.service';
import { ToastrService } from 'ngx-toastr';
import { ProficiencyComponent } from '../proficiency/proficiency.component';
import { LoaderService } from '../../../services/loader.service';
import { MockInterviewDetail } from '../../dto/interviewdetails-response';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-interview-details1',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './interview-details.component.html',
  styleUrl: './interview-details.component.css',
})
export class InterviewDetailsComponent1 {
  interviewDetail!: MockInterviewDetail;
  isLoading = true;
  errorMessage = '';

  constructor(private academyHttpService: AcademyHttpService,private route:ActivatedRoute) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const interviewId = params.get('id');

      if (interviewId) {
        this.fetchInterviewDetails(interviewId);
      }
    });
  }

  fetchInterviewDetails(interview_id: string) {
    this.academyHttpService
      .getInterviewDetail(interview_id)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.isLoading = false;
          this.errorMessage = `Error ${error.status}: ${error.message}`;
          return of(); // Return an empty observable to gracefully complete
        })
      )
      .subscribe((data: any) => {
        if (data) {
          this.interviewDetail = data;
          this.isLoading = false;
        }
      });
  }

  
}
