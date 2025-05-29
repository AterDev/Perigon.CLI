import { Component, EventEmitter, Input, Output } from '@angular/core';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'sync-button',
  imports: [MatProgressSpinnerModule, MatButton],
  templateUrl: './sync-button.component.html',
  styleUrl: './sync-button.component.css'
})
export class SyncButtonComponent {
  @Input() isSync: boolean;
  @Input() text: string = '';
  @Output() click: EventEmitter<void> = new EventEmitter();

  constructor() {
    this.isSync = false;
  }
}
