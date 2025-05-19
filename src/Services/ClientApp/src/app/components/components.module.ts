import { NgModule } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTreeModule } from '@angular/material/tree';
import { MatSliderModule } from '@angular/material/slider';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatRadioModule } from '@angular/material/radio';
import { LayoutComponent } from './layout/layout.component';
import { NavigationComponent } from './navigation/navigation.component';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { SyncButtonComponent } from './sync-button/sync-button.component';
import { ChatBotComponent } from './chatbot/chatbot.component';
import { MarkdownModule } from 'ngx-markdown';
import { FormsModule } from '@angular/forms';
import { QuickNavComponent } from './quick-nav/quick-nav.component';
import { CustomPaginatorIntl } from './CustomPaginatorIntl';
import { TypedCellDefDirective } from './typed-cell-def.directive';

const MaterialModules = [
  MatToolbarModule,
  MatMenuModule,
  MatButtonModule,
  MatFormFieldModule,
  MatInputModule,
  MatCardModule,
  MatSidenavModule,
  MatListModule,
  MatIconModule,
  MatSnackBarModule,
  MatSelectModule,
  MatNativeDateModule,
  MatDatepickerModule,
  MatTooltipModule,
  MatExpansionModule,
  MatDialogModule,
  MatTabsModule,
  MatTableModule,
  MatPaginatorModule,
  MatStepperModule,
  MatCheckboxModule,
  MatProgressSpinnerModule,
  MatProgressBarModule,
  MatSortModule,
  MatButtonToggleModule,
  MatBadgeModule,
  MatTreeModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatChipsModule,
  MatAutocompleteModule,
  MatRadioModule,
  MatBottomSheetModule,
];

const components = [
  LayoutComponent,
  NavigationComponent,
  ConfirmDialogComponent,
  AdminLayoutComponent,
  SyncButtonComponent,
  ChatBotComponent
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ...MaterialModules,
    MarkdownModule.forRoot(),
    QuickNavComponent,
    ...components, TypedCellDefDirective
  ],
  providers: [
    { provide: MatPaginatorIntl, useClass: CustomPaginatorIntl },
  ],
  exports: [
    CommonModule,
    RouterModule,
    TypedCellDefDirective,
    ...MaterialModules,
    ...components
  ]
})
export class ComponentsModule { }