import { Component, Inject, PLATFORM_ID } from "@angular/core";
import { isPlatformBrowser } from "@angular/common";
import { NavigationEnd, Router, RouterOutlet } from "@angular/router";
import { SidebarComponent } from "@shared/component/sidebar/sidebar.component";
import { HeaderComponent } from "@shared/component/header/header.component";
import { ScriptLoaderServiceService } from "@shared/component/sidebar/script-loader-service.service";

@Component({
  selector: "app-landing",
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent],
  templateUrl: "./landing.component.html",
  styleUrl: "./landing.component.scss",
})
export class LandingComponent {
  toggleEventValue: string = "";
  count = 0;
  isErrorRoute: boolean = false;
  showMainLayout = true;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private scriptLoader: ScriptLoaderServiceService,
    private router: Router
  ) {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.showMainLayout = !event.url.includes("/error/");
      }
    });
  }

  getToggleEvent(value: any) {
    if (value == "true" && this.count > 0) {
      value = "false";
    } else {
      this.count = this.count + 1;
    }
    // console.log(value , this.count);
    this.toggleEventValue = value;

    if (isPlatformBrowser(this.platformId)) {
      this.scriptLoader
        .loadScript("assets/js/main.min.js")
        .then(() => {
          console.log("Script loaded successfully.");
        })
        .catch((error) => {
          console.error(error);
        });
    }
  }
}
