import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function fileValidator(maxFileSizeInMB: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    
    if (!control.value) {
      return null;
    }

    const file = control.value as File;

    const allowedTypes = [
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    ];
    if (!allowedTypes.includes(file.type)) {
      return {
        invalidFileType: 'Only .pdf, .doc, and .docx files are allowed',
      };
    }

    const maxSize = maxFileSizeInMB * 1048576;
    if (file.size > maxSize) {
      return {
        fileTooLarge: `Exceeds maximum allowed size (${maxFileSizeInMB} MB)`,
      };
    }

    return null;
  };
}
