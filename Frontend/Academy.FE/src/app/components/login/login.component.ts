import { LoaderService } from "@services/loader.service";
import { SocialUser } from "@abacritt/angularx-social-login";
import { isPlatformBrowser } from "@angular/common";
import {
  AfterViewInit,  Component,  CUSTOM_ELEMENTS_SCHEMA,  Inject,
  NgZone,  OnInit,  PLATFORM_ID
} from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { CredentialResponse, PromptMomentNotification } from "google-one-tap";
import { filter, finalize } from "rxjs";
import { environment } from "@environments/environment";
import { AcademyHttpService } from "@services/academy-http.service";
import { AuthenticationService } from "@services/authentication.service";
import { AcademyResponse } from "@shared/dto/academy-response.dto";

@Component({
  selector: "app-login",
  standalone: true,
  imports: [], // Import SocialLoginModule here
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.scss",
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class LoginComponent implements OnInit, AfterViewInit {
  users: string = "";
  googleClientId: string = "";
  user: SocialUser = new SocialUser();
  private googleScriptLoaded = false;
  private routerSubscription: any;

  constructor(
    private router: Router,
    private ngZone: NgZone,
    private authenticationService: AuthenticationService,
    private loader: LoaderService,
    private readonly academyHttpService: AcademyHttpService,
    @Inject(PLATFORM_ID) private platformId: Object // Inject PLATFORM_ID to check if it's browser or server
  ) {
    this.googleClientId = environment.GoogleProviderClientId;
  }

  ngOnInit(): void {
    this.authenticationService.isLoggedOut$.subscribe(
      (isLoggedOut: boolean) => {
        if (isLoggedOut) {
          this.loadGoogleScript();
        }
      }
    );
  }

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.routerSubscription = this.router.events
        .pipe(filter((event) => event instanceof NavigationEnd))
        .subscribe(() => {
          this.ngZone.runOutsideAngular(() => {
            if (!this.googleScriptLoaded) {
              this.loadGoogleScript();
              this.googleScriptLoaded = true;
            } else {
              this.renderGoogleButton();
            }
          });
        });
    }
  }

  private loadGoogleScript() {
    if (isPlatformBrowser(this.platformId)) {
      const script = document.createElement("script");
      script.src = "https://accounts.google.com/gsi/client";
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.ngZone.runOutsideAngular(() => {
          this.renderGoogleButton();
        });
      };
      document.body.appendChild(script);
    }
  }

  private renderGoogleButton() {
    if (isPlatformBrowser(this.platformId)) {
      if (
        typeof google === "undefined" ||
        !google.accounts ||
        !google.accounts.id
      ) {
        console.error("Google API not fully loaded. Retrying...");
        setTimeout(() => this.renderGoogleButton(), 200); // Retry after a short delay
        return;
      }
      google.accounts.id.initialize({
        client_id: environment.GoogleProviderClientId,
        callback: this.handleCredentialResponse.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true,
      });

      const buttonDiv = document.getElementById("buttonDiv");
      if (buttonDiv) {
        // Render Google sign-in button
        // @ts-ignore
        google.accounts.id.renderButton(buttonDiv, {
          theme: "filled_blue",
          size: "large",
          type: "standard",
          logo_alignment: "center",
          shape: "pill",
          text: "signin",
          width: 300,
          locale: "en",
        });
      }

      google.accounts.id.prompt();
    }
  }

  ngOnDestroy() {
    if (isPlatformBrowser(this.platformId)) {
      if (this.routerSubscription) {
        this.routerSubscription.unsubscribe();
      }
      const googleScript = document.querySelector(
        'script[src="https://accounts.google.com/gsi/client"]'
      );
      if (googleScript) {
        googleScript.remove();
      }
    }
  }

  initiateLogin() {
    if (isPlatformBrowser(this.platformId)) {
      //@ts-ignore
      window.onGoogleLibraryLoad = () => {
        // Initialize Google sign-in library if in browser
        // @ts-ignore
        google.accounts.id.initialize({
          client_id: environment.GoogleProviderClientId,
          callback: this.handleCredentialResponse.bind(this),
          auto_select: false,
          cancel_on_tap_outside: true,
        });

        // Get the element and check if it exists before rendering the button
        const buttonDiv = document.getElementById("buttonDiv");
        if (buttonDiv) {
          // Render Google sign-in button
          // @ts-ignore
          google.accounts.id.renderButton(buttonDiv, {
            theme: "filled_blue",
            size: "large",
            type: "standard",
            logo_alignment: "left",
            shape: "pill",
            text: "signin",
            width: 310,
            locale: "en",
          });
        }

        // Display the prompt
        // @ts-ignore
        google.accounts.id.prompt((notification: PromptMomentNotification) => {
          if (notification.isNotDisplayed()) {
            console.error(
              "GSI Prompt Not Displayed:",
              notification.getNotDisplayedReason()
            );
          } else if (notification.isSkippedMoment()) {
            console.error(
              "GSI Prompt Skipped Moment:",
              notification.getSkippedReason()
            );
          } else if (notification.isDismissedMoment()) {
            console.error(
              "GSI Prompt Dismissed Moment:",
              notification.getDismissedReason()
            );
          }
        });
      };
    }
  }

  parseJwt(token: string) {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map(function (c) {
          return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join("")
    );
    return JSON.parse(jsonPayload);
  }

  async handleCredentialResponse(response: CredentialResponse) {
    const credential = response.credential;
    const user = this.parseJwt(response.credential);

    const userDetail = {
      email: user.email,
      idToken: credential,
      photoUrl: user.picture,
      name: user.name,
    };

    localStorage.setItem("user", JSON.stringify(userDetail));
    localStorage.setItem("idToken", userDetail.idToken);

    if (user) {
      this.authenticationService.isFirstLoggedIn = true;
      this.loader.start();
      this.academyHttpService
        .authenticateUser(userDetail.idToken)
        .pipe(finalize(() => this.loader.stop()))
        .subscribe({
          next: (response: AcademyResponse) => {
            if (response?.success) {
              localStorage.setItem("authToken", response.data);
              this.authenticationService.decodeTokenandReadClaim();
              this.authenticationService.setLogout(false);

              const attemptedUrl = localStorage.getItem("attemptedUrl");

              this.loader.stop();
              if (attemptedUrl && attemptedUrl.includes("mock-interview")) {
                this.router.navigate([attemptedUrl]);
              } else {
                this.router.navigate(["list"]);
              }
              localStorage.removeItem("attemptedUrl");
            }
          },
        });
    }
  }
}
