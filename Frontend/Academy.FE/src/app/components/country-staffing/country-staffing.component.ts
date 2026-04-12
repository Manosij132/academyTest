import { Component, Input } from '@angular/core';
import { SubMenuItem, subMenuItems } from './country-staffing-constants';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';

@Component({
    selector: 'app-country-staffing',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './country-staffing.component.html',
    styleUrl: './country-staffing.component.css'
})
export class CountryStaffingComponent {
    @Input() getMenuClick?: () => void;
    subMenuItems: SubMenuItem[] = subMenuItems;
    currentUrl: string = '';

    constructor(public router: Router) { }

    ngOnInit(): void {
        this.currentUrl = this.router.url;

        this.router.events.subscribe((event) => {
            if (event instanceof NavigationEnd) {
                this.currentUrl = event.urlAfterRedirects;
            }
        });
    }

    isActive(path: string): boolean {
        const fullPath = path ? `/staffing/${path}` : `/staffing`;
        return (
            this.currentUrl === fullPath ||
            this.currentUrl.startsWith(fullPath + '/')
        );
    }
}
