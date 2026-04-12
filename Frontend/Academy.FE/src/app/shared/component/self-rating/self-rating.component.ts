import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

interface Skill {
  id?: number;
  name: string;
  rating: number;
}

@Component({
  standalone: true,
  selector: 'app-self-rating',
  templateUrl: './self-rating.component.html',
  styleUrls: ['./self-rating.component.css'],
  imports: [CommonModule],
})
export class SelfRatingComponent {

  skills: Skill[] = [];
  evaluationType: string = '';
  profileName: string = '';

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<SelfRatingComponent>
  ) {
    this.skills = data.skills || [];
    this.evaluationType = data.evaluationType || '';
    this.profileName = data.profileName || '';
  }

  setRating(skill: Skill, value: number) {
    skill.rating = value;
  }

  get isSubmitDisabled(): boolean {
    return !this.skills.some(s => s.rating > 0);
  }

  get message(): string {
    if (this.evaluationType === 'Training') {
      return `Hey Glober, This is a self-evaluation of your ${this.profileName} skills. Please rate your proficiency in these skills as it was before you attended the training.`;
    }
    return `This is a self-evaluation for your ${this.evaluationType}. Please rate yourself on the sub-skills listed below.`;
  }

  submitRatings() {
    const payload = this.skills.map(skill => ({
      skillName: skill.name,
      ratingOutOfFive: skill.rating,
    }));

    this.dialogRef.close(payload);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}