import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
    selector: 'app-progress-dialog',
    imports: [MatDialogModule, MatProgressBarModule, MatButtonModule],
    templateUrl: './progress-dialog.component.html',
    styleUrl: './progress-dialog.component.css'
})
export class ProgressDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ProgressDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { title: '', content: '' }
  ) {
  }

  ngOnInit() {
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
  onNoClick(): void {
    this.dialogRef.close();
  }
}
