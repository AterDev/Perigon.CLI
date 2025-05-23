import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
    selector: 'sync-button',
    templateUrl: './sync-button.component.html',
    styleUrl: './sync-button.component.css',
    imports: [MatButton, MatProgressSpinner]
})
export class SyncButtonComponent {
  @Input() isSync: boolean;
  @Input() text: string = '';
  @Output() click: EventEmitter<void> = new EventEmitter();

  constructor() {
    this.isSync = false;
  }
}
