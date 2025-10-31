
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatDialogModule } from '@angular/material/dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';


export const BaseMatModules = [TranslateModule,
  CommonModule, MatIconModule, MatTooltipModule, MatButtonModule, MatProgressSpinnerModule, MatToolbarModule
];
// 表单页面依赖的模块
export const CommonFormModules = [...BaseMatModules, MatFormFieldModule, MatDialogModule, ReactiveFormsModule, FormsModule, MatInputModule, MatSelectModule, MatDatepickerModule];
// 列表页面依赖的模块
export const CommonListModules = [...BaseMatModules, MatTableModule, MatPaginatorModule, MatDialogModule, RouterModule];
export const CommonModules = [CommonModule, RouterModule]
