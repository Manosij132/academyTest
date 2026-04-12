import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Interview, InterviewsService } from '../../../../../services/interviews.service';
import { ToastrService } from 'ngx-toastr';
import { TOASTER_MESSAGES } from '@shared/constants/app.constants';

@Component({
  selector: 'app-deleteinterview',
  templateUrl: './deleteinterview.component.html',
  styleUrl: './deleteinterview.component.css',
  standalone: true,
  imports: [CommonModule]
})
export class DeleteinterviewComponent {
  @Input() interview!: Interview;
  @Output() backToList = new EventEmitter<void>();
  loading = false;
  error: string | null = null;

  constructor(
    private interviewsService: InterviewsService,
    private toastr: ToastrService
  ) {}

  deleteInterview() {
    if (!this.interview?.id) return;
    this.loading = true;
    this.interviewsService.delete(this.interview.id).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.toastr.success(res?.message || TOASTER_MESSAGES.DELETE_SUCCESS, 'Success');
        this.backToList.emit();
      },
      error: () => {
        this.error = 'Failed to delete evaluation.';
        this.loading = false;
      }
    });
  }

  cancel() {
    this.backToList.emit();
  }
}
