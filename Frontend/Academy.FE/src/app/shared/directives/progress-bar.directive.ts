import { Directive, ElementRef, Input, OnChanges, Renderer2, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[appProgressBar]',
  standalone: true
})
export class ProgressBarDirective implements OnChanges {
  @Input() percentage: number = 0;

  constructor(private el: ElementRef, private renderer: Renderer2) { }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['percentage']) {
      this.updateProgressBar();
    }
  }

  private updateProgressBar() {
    this.renderer.setStyle(this.el.nativeElement, 'width', `${this.percentage}%`);
    this.renderer.setStyle(this.el.nativeElement, 'backgroundColor', '#76c7c0');
  }
}
