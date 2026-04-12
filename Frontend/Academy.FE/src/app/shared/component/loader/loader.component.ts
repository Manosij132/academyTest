import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { LoaderService } from '../../../services/loader.service';
import { Observable, Subscription } from 'rxjs';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.scss'
})
export class LoaderComponent implements OnInit, OnDestroy {
  loading$!: Observable<boolean>;
  private sub!: Subscription;

  constructor(
    private loaderService: LoaderService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit() {
    this.loading$ = this.loaderService.loaderState;

    this.sub = this.loaderService.loaderState.subscribe(isLoading => {
      if (isPlatformBrowser(this.platformId)) {
        if (isLoading) {
          setTimeout(() => {
            (document.activeElement as HTMLElement)?.blur();
          });
          document.body.style.overflow = 'hidden';
        } else {
          document.body.style.overflow = 'auto';
        }
      }
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
    if (isPlatformBrowser(this.platformId)) {
      document.body.style.overflow = 'auto';
    }
  }
}
