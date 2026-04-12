import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { EvaluataionReportService } from '../../../services/evaluation-report.service';
import FileSaver from 'file-saver';
import { ToastrService } from 'ngx-toastr';
import { TOASTER_MESSAGES } from "@shared/constants/app.constants";

@Component({
  selector: 'app-evaluation-report',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, CommonModule],
  templateUrl: './evaluation-report.component.html',
  styleUrl: './evaluation-report.component.css'
})
export class EvaluationReportComponent {

  constructor(private reportService: EvaluataionReportService, private toastr: ToastrService) {}

  downloadReport() {

    this.reportService.downloadReport().subscribe({
      next: (response) => {
        let fileName = 'Interview_Evaluation_Report.xlsx';
        
        const contentDisposition = response.headers.get('content-disposition');

        if (contentDisposition) {
          const fileNameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
          const matches = fileNameRegex.exec(contentDisposition);
          if (matches != null && matches[1]) { 
            fileName = matches[1].replace(/['"]/g, '');
          }
        }

        if (response.body) {
          FileSaver.saveAs(response.body, fileName);
          this.toastr.success('Report downloaded successfully.', 'Success');
        }
        
      },
      error: (err) => {
         this.toastr.error('Report Download failed.', 'Error');
        console.error('Download failed', err);
      }
    });
  }

}