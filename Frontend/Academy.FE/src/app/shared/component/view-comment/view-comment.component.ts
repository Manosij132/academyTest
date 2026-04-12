import { Component, Input, OnInit, SimpleChanges } from "@angular/core";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { LoaderService } from "../../../services/loader.service";
import { ToastrService } from "ngx-toastr";
import { CommonModule } from "@angular/common";
import { finalize } from "rxjs";

@Component({
  selector: "app-view-comment",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./view-comment.component.html",
  styleUrl: "./view-comment.component.scss",
})
export class ViewCommentComponent implements OnInit {
  constructor(
    private loaderService: LoaderService,
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService
  ) {}
  comments: any[] = [];
  
  //set values to true post fetching the comment from service
  isCommentsVissible : boolean = false;

  @Input() employeeId: number = 0;
  ngOnInit() {
    this.fetchComments();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes["employeeId"]) {
      this.fetchComments();
    }
  }

  fetchComments() {
    if (this.employeeId == 0) return;
    this.academyHttpService
      .fetchAllComments(this.employeeId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.comments = response.data;
            this.isCommentsVissible = true;
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
        
      });
  }
}
