import { HttpErrorResponse, HttpResponse, type HttpInterceptorFn } from "@angular/common/http";
import { Inject, inject, PLATFORM_ID } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { catchError, map, throwError } from "rxjs";

export interface ApiResponse<T> {
  data: T;
  status: number;
  success: boolean;
  error: {
    code: string;
    message: string;
  };
  stackTrace: string;
}

export const ApiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastrService);
  const platformId = Inject(PLATFORM_ID);
  let errorMessage = "";
  let toastrOptions = {
    timeOut: 5000, // Duration in milliseconds
    closeButton: true,
    positionClass: "toast-top-right", // Adjust position as needed
    progressBar: true,
  };

  const notifyError = (codeText: string, errorMessage: string, error: HttpErrorResponse): void => {
    toastr.error(errorMessage, codeText, toastrOptions);
  };

  return next(req).pipe(
    map((event) => {
      // Only process if it's an HttpResponse
      if (event instanceof HttpResponse) {
        const body = event.body as ApiResponse<any>;

        // Check if the response matches your API structure and Success is false
        if (body && typeof body.success === "boolean" && !body.success) {
          const errorMessage = body.error?.message || "An unknown API error occurred.";
          const errorCode = body.error?.code || "UNKNOWN_API_ERROR";

          // Show a user-friendly message (optional)
          toastr.error(errorMessage, errorCode, toastrOptions);

          // Re-throw an HttpErrorResponse to be caught by catchError in services/components
          const errorResponse = new HttpErrorResponse({
            error: {
              message: errorMessage,
              code: errorCode,
              originalResponse: body, // Keep original response for debugging
            },
            status: body.status || 400, // Use API's status or default to 400
            statusText: "API Error",
            url: req.url, // Use req.url for the URL
          });
          throw errorResponse; // This will trigger the catchError below
        }
      }
      return event;
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
        notifyError("Client Error", errorMessage, error);
      } else {
        // Server-side error
        switch (error.status) {
          case 0:
            errorMessage = "A network error occurred. Please check your internet connection.";
            notifyError("Network Error", errorMessage, error);
            break;
          case 200:
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            notifyError("Server Error", errorMessage, error);
            break;
          case 400:
            errorMessage = error.error?.message || "Bad Request";
            notifyError("Bad Request", errorMessage, error);
            break;
          case 401:
            // 401 errors are handled exclusively by AuthInterceptor
            break;
          case 403:
            errorMessage = error.error?.message || "Forbidden";
            notifyError("Forbidden", errorMessage, error);
            break;
          case 404:
            errorMessage = error.error?.message || "Not Found";
            notifyError("Not Found", errorMessage, error);
            break;
          case 409:
            errorMessage = error.error?.message || "Candidate has an active evaluation for this profile. Complete it before scheduling a new one.";
            toastr.error(errorMessage, "Already Scheduled", {
              timeOut: 5000,
              closeButton: true,
              positionClass: "toast-top-right",
              progressBar: true,
            });
            break;
          case 500:
            errorMessage = error.error?.message || "Internal Server Error";
            notifyError("Internal Server Error", errorMessage, error);
            break;
          default:
            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
            notifyError("Server Error", errorMessage, error);
        }
      }

      return throwError(() => error); // Re-throw the error so the component can handle it if needed
    })
  );
};
