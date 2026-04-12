import { CommonModule } from '@angular/common';
import { Component, ElementRef, forwardRef, Input, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FileUploadComponent),
      multi: true,
    },
  ],
  standalone: true,
  imports: [CommonModule, MatIconModule]
})
export class FileUploadComponent implements ControlValueAccessor {
  @Input() placeholder: string = 'Upload File';
  @ViewChild('fileInput') fileInput!: ElementRef;

  file: File | null = null;
  disabled: boolean = false;

  private onChange = (_: File | null) => {};
  private onTouched = () => {};

  writeValue(file: File | null): void {
    this.file = file;
  }

  registerOnChange(fn: (file: File | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] || null;

    this.file = file;
    this.onChange(file);
    this.onTouched();
  }

  clearFile(): void {
    this.file = null;
    this.fileInput.nativeElement.value = '';
    this.onChange(this.file);
  }

  downloadFile(): void {
    if (this.file) {
      const url = URL.createObjectURL(this.file);
      const a = document.createElement('a');
      a.href = url;
      a.download = this.file.name;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    }
  }
}
