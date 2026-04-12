import { Component, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MockInterviewMenuComponent } from '../mock-interview-menu/mock-interview-menu.component';

@Component({
  selector: 'app-mock-interview-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, MockInterviewMenuComponent],
  templateUrl: './mock-interview-layout.component.html',
  styleUrl: './mock-interview-layout.component.css',
  encapsulation: ViewEncapsulation.None
})
export class MockInterviewLayoutComponent {

}
