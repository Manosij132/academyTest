import type {
  HttpErrorResponse,
  HttpInterceptorFn,
} from "@angular/common/http";
import { inject } from "@angular/core";
import { AuthenticationService } from "../services/authentication.service";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { ToastrService } from "ngx-toastr";

let isSessionExpiredToastShown = false;

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthenticationService);
  const router = inject(Router);
  const toastr = inject(ToastrService);

  if (
    req.url.includes("accounts.google.com") ||
    req.url.includes("apis.google.com")
  ) {
    return next(req); // Do not intercept
  } else if (req.url.includes("/authenticate")) {
    const idToken = authService.fetchIdToken();
    if (idToken) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${idToken}`,
        },
      });
    } else {
      // logout
    }
  } else {
    const token = authService.fetchToken();
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      });
    } else {
      // logout
    }
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        if (!isSessionExpiredToastShown) {
          isSessionExpiredToastShown = true;
          toastr.error("Your session is no longer valid. Please log in again.", "Session Expired");
          authService.setLogout(true);

          setTimeout(() => {
            isSessionExpiredToastShown = false;
            router.navigate(["/login"]);
          }, 2000);
        }
      } else if (error.status === 403) {
        console.error("403 Error:", error);
      } else if (error.status === 500) {
        console.log("500 Error:", error);
      }
      return throwError(() => error);
    })
  );
};
