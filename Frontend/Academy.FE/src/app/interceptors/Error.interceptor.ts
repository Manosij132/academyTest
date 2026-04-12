import { isPlatformBrowser } from "@angular/common";
import type {
  HttpErrorResponse,
  HttpInterceptorFn,
} from "@angular/common/http";
import { Inject, inject, PLATFORM_ID } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { catchError, throwError } from "rxjs";

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastrService);
  const platformId = Inject(PLATFORM_ID);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = "";
      let toastrOptions = {
        timeOut: 5000, // Duration in milliseconds
        closeButton: true,
        positionClass: "toast-bottom-center", // Adjust position as needed
        progressBar: true,
      };

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
        console.error("Client-side error:", error); // Log for debugging
        toastr.error(errorMessage, "Client Error", toastrOptions);
      } else {
        // Server-side error
        switch (error.status) {
          case 0:
            errorMessage =
              "A network error occurred. Please check your internet connection.";
            console.error("Network Error:", error);
            toastr.error(errorMessage, "Network Error", toastrOptions);
            break;
          case 200:
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            console.error("Server-side error:", error);
            // toastr.error(errorMessage, "Network Error", toastrOptions);
            break;
          case 400:
            errorMessage = error.error?.message || "Bad Request";
            console.error("Bad Request Error:", error);
            toastr.error(errorMessage, "Bad Request", toastrOptions);
            break;
          case 401:
            errorMessage = error.error?.message || "Unauthorized";
            console.error("Unauthorized Error:", error);
            toastr.warning(errorMessage, "Unauthorized", toastrOptions);
            break;
          case 403:
            errorMessage = error.error?.message || "Forbidden";
            console.error("Forbidden Error:", error);
            toastr.warning(errorMessage, "Forbidden", toastrOptions);
            break;
          case 404:
            errorMessage = error.error?.message || "Not Found";
            console.error("Not Found Error:", error);
            toastr.warning(errorMessage, "Not Found", toastrOptions);
            break;
          case 500:
            errorMessage = error.error?.message || "Internal Server Error";
            console.error("Internal Server Error:", error);
            toastr.error(errorMessage, "Server Error", toastrOptions);
            break;
          default:
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            console.error("Server-side error:", error);
            toastr.error(errorMessage, "Server Error", toastrOptions);
        }
      }

      return throwError(() => error); // Re-throw the error so the component can handle it if needed
    })
  );
};
