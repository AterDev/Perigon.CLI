import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { FormArray, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable, forkJoin, lastValueFrom, map, startWith } from 'rxjs';
import { GenActionService } from 'src/app/services/gen-action/gen-action.service';
import { GenActionFilterDto } from 'src/app/services/gen-action/models/gen-action-filter-dto.model';
import { GenActionItemDtoPageList } from 'src/app/services/gen-action/models/gen-action-item-dto-page-list.model';
import { GenActionItemDto } from 'src/app/services/gen-action/models/gen-action-item-dto.model';
import { GenActionAddDto } from 'src/app/services/gen-action/models/gen-action-add-dto.model';
import { GenActionUpdateDto } from 'src/app/services/gen-action/models/gen-action-update-dto.model';
import { GenStepItemDto } from 'src/app/services/gen-step/models/gen-step-item-dto.model';
import { GenStepService } from 'src/app/services/gen-step/gen-step.service';
import { CdkDragDrop, moveItemInArray, transferArrayItem, CdkDropList, CdkDrag } from '@angular/cdk/drag-drop';
import { GenSourceType } from 'src/app/services/enum/models/gen-source-type.model';
import { ModelFileItemDto } from 'src/app/services/gen-action/models/model-file-item-dto.model';
import { GenActionRunDto } from 'src/app/services/gen-action/models/gen-action-run-dto.model';
import { MatToolbar, MatToolbarRow } from '@angular/material/toolbar';
import { MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { NgIf, NgFor } from '@angular/common';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { TypedCellDefDirective } from '../../../components/typed-cell-def.directive';
import { MatDivider } from '@angular/material/divider';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatSelect } from '@angular/material/select';
import { MatOption } from '@angular/material/core';
import { MatAutocompleteTrigger, MatAutocomplete } from '@angular/material/autocomplete';
import { SyncButtonComponent } from '../../../components/sync-button/sync-button.component';
import { ToKeyValuePipe } from '../../../share/pipe/to-key-value.pipe';
import { EnumTextPipe } from '../../../pipe/enum-text.pipe';

@Component({
    selector: 'app-index',
    templateUrl: './task.component.html',
    styleUrls: ['./task.component.scss'],
    imports: [MatToolbar, MatToolbarRow, MatFormField, MatLabel, MatInput, FormsModule, MatButton, MatTooltip, MatIcon, NgIf, MatProgressSpinner, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, TypedCellDefDirective, MatCellDef, MatCell, MatIconButton, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow, MatDivider, MatPaginator, MatDialogTitle, CdkScrollable, MatDialogContent, ReactiveFormsModule, MatSelect, MatOption, NgFor, MatDialogActions, MatDialogClose, CdkDropList, CdkDrag, MatAutocompleteTrigger, MatSuffix, MatAutocomplete, SyncButtonComponent, ToKeyValuePipe, EnumTextPipe]
})
export class TaskComponent implements OnInit {
  GenSourceType = GenSourceType;
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: GenActionItemDto[] = [];
  models: ModelFileItemDto[] = [];
  filteredModels: ModelFileItemDto[] = [];
  columns: string[] = ['name', 'description', 'sourceType', 'status', 'actions'];
  dataSource!: MatTableDataSource<GenActionItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('addDialog', { static: true }) addTmpl!: TemplateRef<{}>;
  @ViewChild('addStepDialog', { static: true }) addStepTmpl!: TemplateRef<{}>;
  @ViewChild('runDialog', { static: true }) runTmp!: TemplateRef<{}>;
  isEditable = false;
  addForm!: FormGroup;
  addDto: GenActionAddDto | null = null;
  currentItem = {} as GenActionItemDto;
  filter: GenActionFilterDto;
  pageSizeOption = [12, 20, 50];

  runFilePath: string | null = null;

  // 选择steps
  allSteps: GenStepItemDto[] = [];
  remainSteps: GenStepItemDto[] = [];
  selectedSteps: GenStepItemDto[] = [];

  constructor(
    private service: GenActionService,
    private stepService: GenStepService,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.filter = {
      pageIndex: 1,
      pageSize: 12,
      name: null
    };
  }

  get name(): FormControl { return this.addForm.get('name') as FormControl; }
  get description(): FormControl { return this.addForm.get('description') as FormControl; }
  get entityPath(): FormControl { return this.addForm.get('entityPath') as FormControl; }
  get openApiPath(): FormControl { return this.addForm.get('openApiPath') as FormControl; }
  get sourceType(): FormControl { return this.addForm.get('sourceType') as FormControl; }
  get variables(): FormArray { return this.addForm.get('variables') as FormArray; }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
      .subscribe({
        next: ([res]) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<GenActionItemDto>(this.data);
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

  getListAsync(): Observable<GenActionItemDtoPageList> {
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
              this.dataSource = new MatTableDataSource<GenActionItemDto>(this.data);
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

  async getModels(type: GenSourceType): Promise<void> {
    if (type != null) {
      const res = await lastValueFrom(this.service.getModelFile(type));
      if (res) {
        this.models = res;
        this.filteredModels = this.models;
      }
    }
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
      name: new FormControl(null, [Validators.required, Validators.maxLength(40)]),
      description: new FormControl(null, [Validators.maxLength(1024)]),
      entityPath: new FormControl(null, [Validators.maxLength(1024)]),
      openApiPath: new FormControl(null, [Validators.maxLength(200)]),
      sourceType: new FormControl(null),
      variables: new FormArray([])
    });
  }

  filterModels(value: string): void {
    const filterValue = value;
    this.filteredModels = this.models.filter(o => o.name.toLowerCase().includes(filterValue));
  }

  async openAddDialog(item: GenActionItemDto | null = null, isEditable = false): Promise<void> {
    this.initForm();
    this.isEditable = isEditable;
    if (this.isEditable && item) {
      this.currentItem = item;
      this.name?.setValue(item?.name);

      this.openApiPath?.setValue(item?.openApiPath);
      this.sourceType?.setValue(item?.sourceType);
      this.description?.setValue(item?.description);

      this.entityPath.setValue(this.models.find(_ => _.fullName === item.entityPath)?.name);

      if (item.variables) {
        const groups = item.variables.map(item => new FormGroup({
          key: new FormControl(item.key, [Validators.required, Validators.maxLength(100)]),
          value: new FormControl(item.value, [Validators.required, Validators.maxLength(1000)]),
        }));

        groups.forEach(group => {
          this.variables.push(group);
        });
      }

    }
    this.dialogRef = this.dialog.open(this.addTmpl, {
      minWidth: '400px',
      maxHeight: '98vh'
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }

  drop(event: CdkDragDrop<GenStepItemDto[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
    }
  }

  openAddStep(item: GenActionItemDto): void {
    this.currentItem = item;
    forkJoin([this.getAllSteps(), this.getActionSteps()])
      .subscribe({
        next: ([allSteps, actionSteps]) => {
          if (allSteps.data) {
            this.allSteps = allSteps.data;
            this.selectedSteps = actionSteps;
            this.remainSteps = this.allSteps.filter(_ => !this.selectedSteps.some(s => s.id === _.id));

            // 弹窗
            this.dialogRef = this.dialog.open(this.addStepTmpl, {
              minWidth: '400px',
              maxHeight: '98vh'
            })
            // this.dialogRef.afterClosed();
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {

        }
      });
  }

  async openRunDialog(item: GenActionItemDto): Promise<void> {
    this.currentItem = item;
    if (item.sourceType != null) {
      await this.getModels(item.sourceType);
      this.dialogRef = this.dialog.open(this.runTmp, {
        minWidth: '400px',
        maxHeight: '98vh'
      });
    } else {
      this.execute();
    }
  }

  saveSteps(): void {
    this.service.addSteps(this.currentItem.id, this.selectedSteps.map(_ => _.id))
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

  getAllSteps() {
    return this.stepService.filter({
      pageIndex: 1,
      pageSize: 999
    });
  }

  getActionSteps() {
    return this.service.getSteps(this.currentItem.id);
  }

  addVariable(): void {
    this.variables.push(new FormGroup({
      key: new FormControl(null, [Validators.required, Validators.maxLength(100)]),
      value: new FormControl(null, [Validators.required, Validators.maxLength(1000)])
    }));
  }

  removeVariable(index: number): void {
    this.variables.removeAt(index);
  }

  save(): void {
    if (this.addForm.invalid) {
      this.snb.open('请检查输入项');
      return;
    }
    this.entityPath.setValue(this.models.find(_ => _.name === this.entityPath.value)?.fullName);

    if (this.isEditable) {
      const data = this.addForm.value as GenActionUpdateDto;
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
      this.addDto = this.addForm.value as GenActionAddDto;
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

  execute(): void {
    this.isProcessing = true;
    if (!this.currentItem.id) {
      this.snb.open('请选择要执行的任务');
      return;
    }
    let path = this.runFilePath;
    if (this.currentItem.sourceType !== GenSourceType.OpenAPI) {
      path = this.models.find(_ => _.name === this.runFilePath)?.fullName ?? null;
    }
    const data: GenActionRunDto = {
      id: this.currentItem.id,
      sourceFilePath: path,
      onlyOutput: false
    }
    this.service.execute(data)
      .subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.snb.open('执行成功');
          } else {
            this.snb.open('执行失败:' + res.errorMsg);
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {
          this.isProcessing = false;
          this.getList();
        }
      });
  }

  deleteConfirm(item: GenActionItemDto): void {
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
  delete(item: GenActionItemDto): void {
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

  openSaveTemplateDialog(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      data: {
        title: '保存模板',
        content: '保存将覆盖当前模板，是否确定保存?'
      }
    });

    ref.afterClosed().subscribe(res => {
      if (res) {
        this.saveTemplate();
      }
    });
  }
  saveTemplate(): void {
    this.service.saveSyncData()
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('保存成功');
          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  syncTemplate(): void {
    this.service.sync()
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open(res);
            this.getList();
          } else {
            this.snb.open(res);
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

}
