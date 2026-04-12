import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function multiEmailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    // A standard regex for email validation
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    // Split the input string by commas, trim whitespace, and filter out any empty strings
    const emails = control.value
      .split(',')
      .map((email: string) => email.trim())
      .filter((email: string) => email.length > 0);

    // Find any emails that do not match the regex
    const invalidEmails = emails.filter((email: string) => !emailRegex.test(email));

    // If there are any invalid emails, return an error object.
    // The error object contains the list of invalid emails for more detailed feedback.
    if (invalidEmails.length > 0) {
      return { invalidEmails: { value: invalidEmails.join(', ') } };
    }

    // If all emails are valid, return null
    return null;
  };
}