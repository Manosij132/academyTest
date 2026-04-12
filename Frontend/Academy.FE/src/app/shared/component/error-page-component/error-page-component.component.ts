import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-error-page-component',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './error-page-component.component.html',
  styleUrl: './error-page-component.component.css'
})
export class ErrorPageComponentComponent {

}
