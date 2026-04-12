import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TitleService } from '../../../services/title.service';


interface SkillData {
  skillName: string;
  ability: string;
  knowledge: string;
}
@Component({
  selector: 'confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatDialogModule,MatIconModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent implements OnInit {
  popupTitle: string = '';

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, private titleService: TitleService, private dialogRef: MatDialogRef<ConfirmDialogComponent>) {
  }

  ngOnInit(): void {
    this.titleService.title$.subscribe(title => {
      setTimeout(()=>{
        this.popupTitle = title;
      }, 0)
    });
  }

  onSave(){
    this.dialogRef.close('Success');
  }
  
  onClose(){
    this.dialogRef.close('Failed');
  }
}
