import { Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { LoaderService } from "../../../services/loader.service";
import { CommonModule } from "@angular/common";
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { CreateCategoryRequest } from "../../dto/CreateCategoryRequest";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatButtonModule } from "@angular/material/button";
import { finalize } from "rxjs";
import { TOASTER_MESSAGES } from "../../constants/app.constants";

@Component({
  selector: "app-create-category",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: "./create-category.component.html",
  styleUrl: "./create-category.component.scss",
})
export class CreateCategoryComponent implements OnInit {
  categories: any[] = [];
  public categoryForm!: FormGroup;
  constructor(
    private fb: FormBuilder,
    private readonly toastr: ToastrService,
    private readonly academyHttpService: AcademyHttpService,
    private loaderService: LoaderService
  ) {}

  ngOnInit() {
    this.initForm();
    this.loadCategories();
  }

  initForm() {
    this.categoryForm = this.fb.group({
      categoryName: ["", Validators.required],
      parentCategoryId: [0, Validators.required],
    });
  }

  loadCategories() {
    this.loaderService.start();
    this.academyHttpService
      .fetchCategories()
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.categories = response.data;
          } else {
            this.toastr.error(response.errorMessage, "Error");
            this.loaderService.stop();
          }
        },
      });
  }
  onCategoryFormSubmit() {
    let request = new CreateCategoryRequest();
    if (this.categoryForm.valid) {
      request.Name = this.categoryForm.get("categoryName")?.value;
      request.ParentCategoryId =
        this.categoryForm.get("parentCategoryId")?.value;
      this.insertCategoryDetails(request);
    } else {
      this.toastr.error("Please select Required* Fields", "Error");
    }
  }
  insertCategoryDetails(request: CreateCategoryRequest) {
    this.loaderService.start();
    this.academyHttpService
      .insertCategory(request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
            this.initForm();
            this.loadCategories();
          } else {
            this.toastr.error(response.errorMessage, "Error");
            this.loaderService.stop();
          }
        },
      });
  }
}
