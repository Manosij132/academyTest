
import { CommonModule } from "@angular/common";
import { Component, Inject, OnInit } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MatTableModule } from "@angular/material/table";
import { ProficiencyComponent } from "../proficiency/proficiency.component";
import { AuthenticationService } from "./../../../services/authentication.service";

interface SkillData {
  skillName: string;
  ability: string;
  knowledge: string;
}

export interface DialogData {
  title?: string;
  message?: string;
  id?: number; // Optional property
  // Add more properties as needed
}

@Component({
  selector: "app-update-ability-knowledge-dialog",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    ProficiencyComponent,
  ],
  templateUrl: "./update-ability-knowledge-dialog.component.html",
  styleUrl: "./update-ability-knowledge-dialog.component.css",
})
export class UpdateAbilityKnowledgeDialogComponent implements OnInit {
  employeeId: any = 0;
  constructor(
    public dialogRef: MatDialogRef<UpdateAbilityKnowledgeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}
  ngOnInit(): void {
    console.log("data", this.data.id);
    this.employeeId = this.data.id;
  }
  displayedColumns: string[] = ["skillName", "ability", "knowledge"];
  skillData: SkillData[] = [
    { skillName: "Angular", ability: "Advanced", knowledge: "High" },
    { skillName: "React", ability: "Intermediate", knowledge: "Moderate" },
    { skillName: "TypeScript", ability: "Advanced", knowledge: "High" },
  ];
}
