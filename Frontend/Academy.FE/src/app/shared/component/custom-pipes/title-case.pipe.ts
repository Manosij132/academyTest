import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'titleCaseCustom',
  standalone: true
})
export class TitleCaseCustomPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';

    return value
      .split(/[-_\s]+/) // Split by space, hyphen, or underscore
      .map(word => {
        if (!word) return ''; 
        // Capitalize first letter + rest of the word in lowercase
        return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
      })
      .join(' '); // Use a space here instead of an empty string
  }
}