import { Component, Inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatTableModule } from "@angular/material/table";
import { MatDialogModule, MatDialogRef } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { CreateSkillComponent } from "@shared/component/create-skill/create-skill.component";
import { TitleService } from "@services/title.service";

interface SkillData {
  skillName: string;
  ability: string;
  knowledge: string;
}
@Component({
  selector: "app-add-skill-dialog",
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    CreateSkillComponent,
  ],
  templateUrl: "./add-skill-dialog.component.html",
  styleUrl: "./add-skill-dialog.component.css",
})
export class AddSkillDialogComponent implements OnInit {
  popupTitle: string = "";

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private titleService: TitleService,
    private dialogRef: MatDialogRef<AddSkillDialogComponent>
  ) {}

  ngOnInit(): void {
    this.titleService.title$.subscribe((title) => {
      setTimeout(() => {
        this.popupTitle = title;
      }, 0);
    });
  }

  onSave() {}

  onClose(success: any) {
    if (success === true) {
      this.dialogRef.close(success);
    }
  }
}
