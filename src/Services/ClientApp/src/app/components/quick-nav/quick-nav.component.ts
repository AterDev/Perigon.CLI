import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-quick-nav',
  imports: [MatButtonModule, MatIconModule, MatTooltipModule],
  standalone: true,
  templateUrl: './quick-nav.component.html',
  styleUrl: './quick-nav.component.scss'
})
export class QuickNavComponent {
  showMore = false;
  dialogRef!: MatDialogRef<{}, any>;
  constructor(
  ) {
  }
  toggle(): void {
    this.showMore = !this.showMore;
  }
}
