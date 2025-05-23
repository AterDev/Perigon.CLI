import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EnumTextPipe } from 'src/app/pipe/enum-text.pipe';
import { StringConvertType } from 'src/app/services/enum/models/string-convert-type.model';
import { ToolsService } from 'src/app/services/tools/tools.service';

import { ToKeyValuePipe } from '../../../share/pipe/to-key-value.pipe';

@Component({
  selector: 'app-string',
  imports: [MatDialogModule, MatButtonModule, MatFormFieldModule, FormsModule, MatInputModule, ToKeyValuePipe, EnumTextPipe],
  templateUrl: './string.component.html',
  standalone: true,
  styleUrl: './string.component.css'
})
export class StringComponent {
  StringConvertType = StringConvertType;
  content: string | null = null;
  result: Map<string, string> | null = null;
  constructor(
    private dialogRef: MatDialogRef<{}, any>,
    private snb: MatSnackBar,
    private service: ToolsService
  ) {
  }
  convertString(type: StringConvertType): void {
    if (type !== StringConvertType.Guid && this.content === null) {
      this.snb.open('请输入字符串！');
      return;
    }
    this.service.convertString(this.content ?? "guid", type)
      .subscribe({
        next: (res) => {
          if (res) {
            this.result = new Map<string, string>(Object.entries(res));
          }
        },
        error: (error) => {
        },
        complete: () => {
        }
      });
  }
}
