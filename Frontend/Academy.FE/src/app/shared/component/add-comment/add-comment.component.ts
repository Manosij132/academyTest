import { Component, ElementRef, Input, NgModule } from "@angular/core";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { CommentRequest } from "../../dto/comment-request";
import { ToastrService } from "ngx-toastr";
import { LoaderService } from "../../../services/loader.service";
import { CommonModule } from "@angular/common";
import { finalize } from "rxjs";
import { TOASTER_MESSAGES } from "../../constants/app.constants";

@Component({
  selector: "app-add-comment",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./add-comment.component.html",
  styleUrl: "./add-comment.component.scss",
})
export class AddCommentComponent {
  @Input() employeeId: number = 0;
  latestComment: any;
  showLatestComment = false;
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private elementRef: ElementRef,
    private toastr: ToastrService,
    private loaderService: LoaderService
  ) {}

  ngOnInit() {
    this.fetchLatestComment();
  }
  fetchLatestComment() {
    this.loaderService.start();
    this.academyHttpService
      .fetchLatestComment(this.employeeId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.latestComment = response.data;
            this.showLatestComment = this.latestComment.commentText !== null;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  clearTextarea() {
    const textarea = this.elementRef.nativeElement.querySelector("#txtNote");
    textarea.value = "";
  }
  onCommentAdded(note: string) {
    if (note.trim() == "") {
      this.toastr.error("Empty values are now allowed.", "Error");
      return;
    }
    let request = new CommentRequest();
    request.EmployeeId = this.employeeId;
    request.CommentText = note;
    this.loaderService.start();
    this.academyHttpService
      .saveComment(request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
            this.fetchLatestComment();
            this.clearTextarea();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
}
