import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

// Adjust path if needed
import { ParsedPrompt } from './prompt-parser';
import { MatIconModule } from '@angular/material/icon';

@Component({
  standalone: true,
  selector: 'app-prompt-details-dialog',
  templateUrl: './prompt-details-dialog.component.html',
  styleUrl: './prompt-details-dialog.component.css',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class PromptDetailsDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ParsedPrompt
  ) {}
}

