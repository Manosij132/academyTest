// global-error-handler.service.ts
import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: any): void {
    // Log the error to the console or send it to a logging server
    console.error('An error occurred:', error);

    // Optionally, implement additional logic here.
    // For example: Display a user-friendly message or log to an external service.
  }
}
