import { Component, TemplateRef, ViewChild } from '@angular/core';
import { FormGroup, FormControl, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom, forkJoin, Observable } from 'rxjs';
import { baseMatModules } from 'src/app/app.config';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { EnumTextPipe } from 'src/app/pipe/enum-text.pipe';
import { GenStepType } from 'src/app/services/enum/models/gen-step-type.model';
import { GenStepService } from 'src/app/services/gen-step/gen-step.service';
import { GenStepAddDto } from 'src/app/services/gen-step/models/gen-step-add-dto.model';
import { GenStepFilterDto } from 'src/app/services/gen-step/models/gen-step-filter-dto.model';
import { GenStepItemDtoPageList } from 'src/app/services/gen-step/models/gen-step-item-dto-page-list.model';
import { GenStepItemDto } from 'src/app/services/gen-step/models/gen-step-item-dto.model';
import { GenStepUpdateDto } from 'src/app/services/gen-step/models/gen-step-update-dto.model';
import { ToKeyValuePipe } from 'src/app/share/pipe/to-key-value.pipe';

@Component({
  selector: 'app-step',
  templateUrl: './step.component.html',
  styleUrl: './step.component.css',
  imports: [baseMatModules, MatTableModule, MatPaginatorModule, MatDividerModule, MatFormFieldModule, MatDialogModule, ReactiveFormsModule, EnumTextPipe, ToKeyValuePipe, FormsModule, MatButtonModule, MatInputModule, MatSelectModule]

})
export class StepComponent {
  GenStepType = GenStepType;
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: GenStepItemDto[] = [];
  columns: string[] = ['name', 'path', 'outputPath', 'actions'];
  dataSource!: MatTableDataSource<GenStepItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('addDialog', { static: true }) addTmpl!: TemplateRef<{}>;
  @ViewChild('helpDialog', { static: true }) helpTmpl!: TemplateRef<{}>;
  isEditable = false;
  addForm!: FormGroup;
  addDto: GenStepAddDto | null = null;
  currentItem = {} as GenStepItemDto;
  filter: GenStepFilterDto;
  pageSizeOption = [12, 20, 50];
  constructor(
    private service: GenStepService,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.filter = {
      pageIndex: 1,
      pageSize: 12,
    };
  }

  get name(): FormControl { return this.addForm.get('name') as FormControl; }
  get content(): FormControl { return this.addForm.get('content') as FormControl; }
  get path(): FormControl { return this.addForm.get('path') as FormControl; }
  get outputPath(): FormControl { return this.addForm.get('outputPath') as FormControl; }
  get genStepType(): FormControl { return this.addForm.get('genStepType') as FormControl; }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
      .subscribe({
        next: ([res]) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<GenStepItemDto>(this.data);
            }
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

  getListAsync(): Observable<GenStepItemDtoPageList> {
    return this.service.filter(this.filter);
  }

  getList(event?: PageEvent): void {
    if (event) {
      this.filter.pageIndex = event.pageIndex + 1;
      this.filter.pageSize = event.pageSize;
    }
    this.service.filter(this.filter)
      .subscribe({
        next: (res) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<GenStepItemDto>(this.data);
            }
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

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }

  initForm(): void {
    this.addForm = new FormGroup({
      name: new FormControl(null, [Validators.maxLength(100), Validators.required]),
      content: new FormControl(null, [Validators.maxLength(100_000)]),
      path: new FormControl('templates/', [Validators.maxLength(400)]),
      outputPath: new FormControl('src/', [Validators.maxLength(400)]),
      genStepType: new FormControl(GenStepType.File, [Validators.required])
    });
  }

  openHelpDialog(): void {
    this.dialog.open(this.helpTmpl, {
      minWidth: '400px',
      maxHeight: '98vh'
    })
  }

  async openAddDialog(id: string | null = null, isEditable = false): Promise<void> {
    this.initForm();
    this.isEditable = isEditable;
    if (this.isEditable && id) {
      const item = await firstValueFrom(this.service.getDetail(id));
      if (item) {
        this.currentItem = item;
        this.name.setValue(item.name);
        this.path.setValue(item.path);
        this.content.setValue(item.content);
        this.outputPath.setValue(item.outputPath);
        this.genStepType.setValue(item.genStepType);
      }
    }
    this.dialogRef = this.dialog.open(this.addTmpl, {
      minWidth: '650px',
      maxHeight: '98vh'
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }

  async copy(id: string): Promise<void> {
    const item = await firstValueFrom(this.service.getDetail(id));
    this.isEditable = false;
    if (item) {
      this.initForm();
      this.currentItem = item;
      this.name.setValue(item.name);
      this.path.setValue(item.path);
      this.content.setValue(item.content);
      this.outputPath.setValue(item.outputPath);
      this.genStepType.setValue(item.genStepType);
    }
    this.dialogRef = this.dialog.open(this.addTmpl, {
      minWidth: '650px',
      maxHeight: '98vh'
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }

  save(): void {
    if (this.addForm.invalid) {
      this.snb.open('请检查输入项');
      return;
    }
    switch (this.genStepType.value) {
      case GenStepType.File:
        if (!this.path.value && !this.content.value) {
          this.snb.open('请填写路径或内容');
          return;
        }
        break;
      case GenStepType.Command:
        if (!this.content.value) {
          this.snb.open('请填写内容');
          return;
        }
        break;
      case GenStepType.Script:
        if (!this.path.value) {
          this.snb.open('请填写路径');
          return;
        }
        break;
      default:
        break;
    }
    if (this.isEditable) {
      const data = this.addForm.value as GenStepUpdateDto;
      this.service.update(this.currentItem.id, data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('更新成功');
              this.dialogRef.close(true);
            } else {
              this.snb.open('更新失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
          }
        });
    } else {
      this.addDto = this.addForm.value as GenStepAddDto;
      this.service.add(this.addDto)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('添加成功');
              this.dialogRef.close(true);
            } else {
              this.snb.open('添加失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
          }
        });
    }

  }

  deleteConfirm(item: GenStepItemDto): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      data: {
        title: '删除',
        content: '是否确定删除?'
      }
    });

    ref.afterClosed().subscribe(res => {
      if (res) {
        this.delete(item);
      }
    });
  }
  delete(item: GenStepItemDto): void {
    this.isProcessing = true;
    this.service.delete(item.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = this.data.filter(_ => _.id !== item.id);
            this.dataSource.data = this.data;
            this.snb.open('删除成功');
          } else {
            this.snb.open('删除失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
  }
}
