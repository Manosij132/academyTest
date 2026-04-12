import { Component, Inject, OnDestroy, PLATFORM_ID } from "@angular/core";
import { 
  NavigationCancel, NavigationEnd, NavigationError, NavigationStart, 
  Router, RouterOutlet 
} from "@angular/router";
import { CommonModule, isPlatformBrowser } from "@angular/common";
import { LoaderComponent } from "@shared/component/loader/loader.component";
import { BgWorkerService } from "@services/bg-worker.service";
import { LoaderService } from "@services/loader.service";
@Component({
  selector: "app-root",
  standalone: true,
  imports: [RouterOutlet, CommonModule, LoaderComponent],
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"], // Fixed typo: styleUrl to styleUrls
})
export class AppComponent implements OnDestroy {
  title = "Academy.UI";
  showHeader: boolean = false;
  isBrowser: boolean;

  constructor(
    private bgWorker: BgWorkerService,
    private readonly router: Router,
    private loaderService: LoaderService,
    @Inject(PLATFORM_ID) private platformId: Object // Inject PLATFORM_ID to check the environment
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId); // Check if it's running in the browser

    if (this.isBrowser) {
      // Subscribe to router events only if running in the browser
      this.router.events.subscribe((event) => {
        this.showHeader = !(
          window.location.pathname.toLowerCase() === "/login"
        );
        this.handleRouterEvent(event);
      });
    }
  }

   private handleRouterEvent(event:any): void {
    if (event instanceof NavigationStart) {
      if (event.url.startsWith('/trainingreport')) {
        this.loaderService.start();
      }
    }
    if (
      event instanceof NavigationEnd ||
      event instanceof NavigationCancel ||
      event instanceof NavigationError
    ) {
      this.loaderService.stop();
    }
  }
  ngOnDestroy(): void {
    this.bgWorker.terminateWorker();
  }

  ngOnInit() {
    this.bgWorker.init();
  }
}

if (typeof Worker !== "undefined") {
  const worker = new Worker(new URL("./app.worker", import.meta.url));
  worker.onmessage = ({ data }) => {
    // console.log(`page got message: ${data}`);
  };
  worker.postMessage("hello");
} else {
  // web workers are not supported
}
