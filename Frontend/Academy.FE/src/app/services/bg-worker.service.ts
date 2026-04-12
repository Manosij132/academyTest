import { Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { AuthenticationService } from "./authentication.service";
import { AcademyHttpService } from "./academy-http.service";
import { AcademyResponse } from "../shared/dto/academy-response.dto";
import { isPlatformBrowser } from "@angular/common";

@Injectable({
  providedIn: "root",
})
export class BgWorkerService {
  worker!: Worker;
  constructor(
    private auth: AuthenticationService,
    private academyService: AcademyHttpService,
     @Inject(PLATFORM_ID) private platformId: Object // Inject PLATFORM_ID to check if it's browser or server
  ) {}
  terminateWorker() {
    if (this.worker !== undefined) this.worker.terminate();
  }

  init() {
    if (isPlatformBrowser(this.platformId)) {
    if (typeof Worker !== undefined) {
      this.worker = new Worker(new URL("./../app.worker", import.meta.url), {
        type: "module",
      });

      this.worker.onmessage = async (e) => {
        await this.refreshAccessToken();
      };
    }}
  }

  async refreshAccessToken() {
    if (this.auth.isLoggedIn) {
      const accessTokenExpiry: any = new Date(
        this.auth.accessTokenExpiry * 1000
      );
      const currentDate: any = new Date();
      const diff = accessTokenExpiry - currentDate;
      if (diff > 0) {
        const secondsTillExpiry = Math.ceil(diff / 1000);
        // console.log(`seconds till expiry : ${secondsTillExpiry}`);
        if (secondsTillExpiry < 60) {
          const result = await new Promise((resolve, reject) => {
            this.academyService.authenticateUser(this.auth.idToken).subscribe({
              next: (response: AcademyResponse) => {
                if (response?.success) {
                  localStorage.setItem("authToken", response.data);
                  resolve(true);
                }
              },
            });
          });
        }
      }
    }
  }
}
