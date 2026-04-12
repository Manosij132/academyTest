import { Component, Input } from '@angular/core';
import { MenuItem,menuItems } from '../mock-interview-constants';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-mock-interview-menu',
  standalone: true,
  imports: [CommonModule,RouterModule],
  templateUrl: './mock-interview-menu.component.html',
  styleUrl: './mock-interview-menu.component.css'
})
export class MockInterviewMenuComponent {
  @Input() getMenuClick?: () => void;
  menuItems: MenuItem[] = menuItems;
  currentUrl: string = '';

  constructor(public router: Router) {}

  ngOnInit(): void {
    this.currentUrl = this.router.url;

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentUrl = event.urlAfterRedirects;
      }
    });
  }

  isActive(path: string): boolean {
    const fullPath = `/mockInterview/${path}`;
    return this.currentUrl === fullPath || this.currentUrl.startsWith(fullPath + '/');
  }
  
  
  
}
