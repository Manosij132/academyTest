import { CommonModule } from "@angular/common";
import { Component, EventEmitter, OnInit, Output } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { LoaderService } from "../../../services/loader.service";
import { TitleService } from "../../../services/title.service";
import { CreateSkillRequest } from "../../dto/create-skill";
import { CreateCategoryComponent } from "../create-category/create-category.component";

@Component({
  selector: "app-create-skill",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    CreateCategoryComponent,
    MatInputModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
  ],
  templateUrl: "./create-skill.component.html",
  styleUrl: "./create-skill.component.scss",
})
export class CreateSkillComponent implements OnInit {
  categories: any[] = [];
  group: boolean = false;
  mandatory: boolean = false;
  request = new CreateSkillRequest();
  showSkillForm: boolean = true;
  public skillForm!: FormGroup;

  @Output() closepopup = new EventEmitter<boolean>();

  constructor(
    private readonly toastr: ToastrService,
    private readonly academyHttpService: AcademyHttpService,
    private loaderService: LoaderService,
    private fb: FormBuilder,
    private titleService: TitleService
  ) {}
  ngOnInit() {
    this.loadCategories();
    this.initForm();
  }
  onMandatoryChange(event: any) {
    this.mandatory = event.value == 1 ? true : false;
    this.group = event.value == "group";
    if (this.group) {
      this.skillForm.get("groupName")?.setValidators(Validators.required);
    } else {
      this.skillForm.get("groupName")?.clearValidators();
    }
    this.skillForm.get("groupName")?.updateValueAndValidity();
  }

  initForm() {
    this.titleService.set("Create Skill");
    this.skillForm = this.fb.group({
      skillName: ["", Validators.required],
      skillDescription: [""],
      category: [null],
      rule: [null],
      groupName: [""], // this will be conditionally required
      specification: [""],
    });
  }

  onSkillFormSubmit(): void {
    this.request = new CreateSkillRequest();
    if (this.skillForm.valid) {
      this.request.CategoryId = this.skillForm.get("category")?.value;
      this.request.Grouping = this.skillForm.get("groupName")?.value;
      this.request.IsActive = true;
      this.request.Mandatory = this.mandatory;
      this.request.SkillDescription =
        this.skillForm.get("skillDescription")?.value;
      this.request.SkillName = this.skillForm.get("skillName")?.value;
      this.request.Specification = this.skillForm.get("specification")?.value;
      this.insertSkillDetails();
      
    } else {
      this.toastr.error("Please select Required* Fields", "Error");
    }
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
          }
        },
      });
  }

  insertSkillDetails() {
    this.loaderService.start();
    this.academyHttpService
      .insertSkill(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.status === 200) {
            this.toastr.success("Success", "Skill saved Successfully");
            this.initForm();
            this.request = new CreateSkillRequest();
            this.closepopup.emit(true);
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }
  onCreateCategoryClicked(event: any) {
    if (event.value == "##") {
      this.showSkillForm = false;
      this.titleService.set("Create Category");
    }
  }
  backToCreateSkillScreen() {
    this.showSkillForm = true;
    this.initForm();
    this.loadCategories();
  }
}
