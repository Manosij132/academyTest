import { CommonModule } from "@angular/common";
import { Component, Input, OnInit } from "@angular/core";
import { MatIconModule } from "@angular/material/icon";
import { MatTableModule } from "@angular/material/table";
import { ToastrService } from "ngx-toastr";
import { finalize } from "rxjs";
import { AcademyHttpService } from "../../../services/academy-http.service";
import { LoaderService } from "../../../services/loader.service";
import {
  KnowledgeMaster,
  ProficiencyMaster,
  TOASTER_MESSAGES,
} from "../../constants/app.constants";
import { ChangeProficiencyRequest } from "../../dto/update-proficiency-request";

interface SkillData {
  skillId: number;
  isMVP: boolean;
  skillName: string;
  ability: string;
  knowledge: string;
  currentProficiency: string;
  expectedProficiency: string;
  currentKnowledge: string;
  expectedKnowledge: string;
}

@Component({
  selector: "app-proficiency",
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule],
  templateUrl: "./proficiency.component.html",
  styleUrl: "./proficiency.component.scss",
})
export class ProficiencyComponent implements OnInit {
  @Input() employeeId: number = 0;
  proficiencies: any;
  updatedProficiencies: [] | undefined;
  ability = ProficiencyMaster;
  knowledge = KnowledgeMaster;
  request = new ChangeProficiencyRequest();
  constructor(
    private readonly academyHttpService: AcademyHttpService,
    private readonly toastr: ToastrService,
    private loaderService: LoaderService
  ) {}

  ngOnInit() {
    this.fetchUserProficiencies();
  }

  // ngOnChanges(changes: SimpleChanges) {
  //   if (changes["employeeId"]) {
  //     this.fetchUserProficiencies();
  //   }
  // }

  fetchUserProficiencies() {
    if (this.employeeId == 0) return;
    this.loaderService.start();
    this.academyHttpService
      .fetchProficiencies(this.employeeId)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.proficiencies = response.data;
          } else {
            this.toastr.error(
              response.errorMessage,
              "Fetch Proficiencies Error"
            );
          }
        },
      });
  }

  saveProficiency() {
    this.loaderService.start();
    this.academyHttpService
      .updateProficiency(this.request)
      .pipe(finalize(() => this.loaderService.stop()))
      .subscribe({
        next: (response: any) => {
          if (response.success) {
            this.toastr.success(TOASTER_MESSAGES.SUCCESS, "Success");
            this.fetchUserProficiencies();
          } else {
            this.toastr.error(response.errorMessage, "Error");
          }
        },
      });
  }

  setProficiency(
    proficiency: any,
    value: number,
    index: number,
    changeType: string
  ) {
    if (changeType == "Ability") {
      this.request.CurrentProficiency = proficiency.currentProficiency;
      this.proficiencies[index].currentProficiency = value + 1;
      this.request.NewProficiency = value + 1;
      this.request.NewKnowledge = proficiency.currentKnowledge;
    } else if (changeType == "Knowledge") {
      this.request.CurrentKnowledge = proficiency.currentKnowledge;
      this.proficiencies[index].currentKnowledge = value + 1;
      this.request.NewKnowledge = value + 1;
      this.request.NewProficiency = proficiency.currentProficiency;
    }
    this.request.EmployeeId = this.employeeId;
    this.request.SkillId = proficiency.skillId;
  }

  setAbility(proficiency: any, value: number, index: number) {
    this.request.CurrentProficiency = proficiency.currentProficiency;
    this.proficiencies[index].currentProficiency = value + 1;
    this.request.EmployeeId = this.employeeId;
    this.request.NewProficiency = value + 1;
    this.request.SkillId = proficiency.skillId;
  }

  setKnowledge(proficiency: any, value: number, index: number) {
    this.request.CurrentKnowledge = proficiency.currentKnowledge;
    this.proficiencies[index].currentKnowledge = value + 1;
    this.request.EmployeeId = this.employeeId;
    this.request.NewKnowledge = value + 1;
    this.request.SkillId = proficiency.skillId;
  }
}
