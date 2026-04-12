import {
  Component,
  Input,
  Output,
  EventEmitter
} from '@angular/core';

import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-video-player',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-player.component.html',
  styleUrl: "./video-player.component.css",

})
export class VideoPlayerComponent {
  @Input() videoUrl!: string;
  @Output() refreshRequested = new EventEmitter<void>();
  onVideoError() {
    // ToDo: If signed URL expired → ask parent to refresh
    this.refreshRequested.emit();
  }
}