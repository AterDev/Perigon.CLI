import { Component, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialog, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { Project } from 'src/app/services/project/models/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { SolutionService } from 'src/app/services/solution/solution.service';
import { SubProjectInfo } from 'src/app/services/solution/models/sub-project-info.model';
import { MatToolbar, MatToolbarRow } from '@angular/material/toolbar';
import { MatButton } from '@angular/material/button';
import { NgIf } from '@angular/common';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { TypedCellDefDirective } from '../../../components/typed-cell-def.directive';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
    selector: 'app-feature',
    templateUrl: './feature.component.html',
    styleUrls: ['./feature.component.css'],
    imports: [MatToolbar, MatToolbarRow, MatButton, NgIf, MatProgressSpinner, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, TypedCellDefDirective, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow, MatDialogTitle, CdkScrollable, MatDialogContent, FormsModule, ReactiveFormsModule, MatFormField, MatLabel, MatInput, MatError, MatDialogActions, MatDialogClose]
})
export class FeatureComponent {
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: SubProjectInfo[] = [];
  columns: string[] = ['name', 'path', 'actions'];
  dataSource!: MatTableDataSource<SubProjectInfo>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('addModuleDialog', { static: true })
  addTmpl!: TemplateRef<{}>;
  mydialogForm!: FormGroup;

  project: Project | null = null;
  pageSizeOption = [12, 20, 50];
  constructor(
    private service: SolutionService,
    private projectSrv: ProjectStateService,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.project = this.projectSrv.project;
  }

  get name() { return this.mydialogForm.get('name'); }

  ngOnInit(): void {
    this.getList();
    this.mydialogForm = new FormGroup({
      name: new FormControl('', [Validators.required, Validators.maxLength(40)]),
    })
  }

  getList(event?: PageEvent): void {
    this.service.getModulesInfo()
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = res;
            this.dataSource = new MatTableDataSource<SubProjectInfo>(this.data);

          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  openModuleDialog(): void {
    this.dialogRef = this.dialog.open(this.addTmpl, {
      width: '300px',
    });
  }

  addModule(): void {
    if (this.mydialogForm.valid) {
      this.isProcessing = true;
      this.service.createModule(this.name?.value)
        .subscribe({
          next: (res) => {
            if (res) {

              this.snb.open('添加成功');
              this.getList();
            } else {
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isProcessing = false;
          },
          complete: () => {
            this.dialogRef.close();
            this.isProcessing = false;
          }
        });
    }
  }
}
