import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CreateEcosystemComponent } from '@shared/component/create-ecosystem/create-ecosystem.component';
import { TitleService } from '@services/title.service';

interface SkillData {
  skillName: string;
  ability: string;
  knowledge: string;
}

@Component({
  selector: 'app-add-ecosystem-dialog',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatDialogModule,MatIconModule, CreateEcosystemComponent],
  templateUrl: './add-ecosystem-dialog.component.html',
  styleUrl: './add-ecosystem-dialog.component.css'
})
export class AddEcosystemDialogComponent implements OnInit {
  popupTitle!: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, private titleService: TitleService) {
  }

  ngOnInit(): void {
    this.titleService.title$.subscribe(title => {
      this.popupTitle = title;
    });
  }

  onSave(){

  }
  
  onClose(){

  }
}
