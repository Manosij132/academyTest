import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-mock-interview-layout-component',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mock-interview-layout-component.component.html',
  styleUrl: './mock-interview-layout-component.component.css'
})
export class MockInterviewLayoutComponentComponent {

}
