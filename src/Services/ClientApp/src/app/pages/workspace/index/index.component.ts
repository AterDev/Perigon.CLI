import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators, FormsModule } from '@angular/forms';
import { MatDialog, MatDialogRef, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatSelectionListChange, MatSelectionList, MatListOption } from '@angular/material/list';
import { MatSelectChange } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { LoginService } from 'src/app/auth/login.service';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { BatchGenerateDto } from 'src/app/services/entity-info/models/batch-generate-dto.model';
import { EntityFile } from 'src/app/services/entity-info/models/entity-file.model';
import { GenerateDto } from 'src/app/services/entity-info/models/generate-dto.model';
import { NgModuleDto } from 'src/app/services/entity-info/models/ng-module-dto.model';
import { CommandType } from 'src/app/services/enum/models/command-type.model';
import { RequestLibType } from 'src/app/services/enum/models/request-lib-type.model';
import { Project } from 'src/app/services/project/models/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { EntityInfoService } from 'src/app/services/entity-info/entity-info.service';
import { ProjectService } from 'src/app/services/project/project.service';
import { ProjectConfig } from 'src/app/services/project/models/project-config.model';
import { ProgressDialogComponent } from 'src/app/components/progress-dialog/progress-dialog.component';
import { SubProjectInfo } from 'src/app/services/solution/models/sub-project-info.model';
import { NgIf, NgFor } from '@angular/common';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatToolbar } from '@angular/material/toolbar';
import { MatIconButton, MatButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatFormField, MatLabel, MatHint } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatCheckbox } from '@angular/material/checkbox';
import { TypedCellDefDirective } from '../../../components/typed-cell-def.directive';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { EditorComponent } from 'ngx-monaco-editor-v2';

@Component({
    selector: 'app-index',
    templateUrl: './index.component.html',
    styleUrls: ['./index.component.css'],
    imports: [NgIf, MatProgressSpinner, MatToolbar, MatIconButton, MatTooltip, MatIcon, MatFormField, MatLabel, MatInput, FormsModule, MatMenu, MatMenuItem, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCheckbox, TypedCellDefDirective, MatCellDef, MatCell, MatButton, MatMenuTrigger, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow, MatDialogTitle, CdkScrollable, MatDialogContent, MatSlideToggle, MatDialogActions, MatDialogClose, MatSelectionList, NgFor, MatListOption, MatHint, EditorComponent]
})
export class IndexComponent implements OnInit {
  RequestLibType = RequestLibType;
  CommandType = CommandType;
  project = {} as Project;
  projectId: string;
  entityFiles = [] as EntityFile[];
  baseEntityPath = '';
  columns: string[] = ['select', 'name', 'description', 'actions'];
  dataSource!: MatTableDataSource<EntityFile>;
  isLoading = true;
  requestForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  searchKey = '';
  isSync = false;
  force = false;
  isListening = false;
  isProcessing = false;
  modules: string[] = [];
  entityName: string | null = null;
  entityDescription: string | null = null;
  selectedService: string | null = null;
  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;
  @ViewChild("syncDialog", { static: true })
  syncTmpRef!: TemplateRef<{}>;
  @ViewChild("protobufDialog", { static: true }) protobufTmpRef!: TemplateRef<{}>;
  @ViewChild("apiDialog", { static: true }) apiTmpRef!: TemplateRef<{}>;
  @ViewChild("generateDialog", { static: true }) generateTmpRef!: TemplateRef<{}>;
  @ViewChild('previewDialog', { static: true }) previewTmpl!: TemplateRef<{}>;
  @ViewChild('ngPagesDialog', { static: true }) ngPagesTmpl!: TemplateRef<{}>;
  @ViewChild('addEntityDialog', { static: true }) addEntityTmpl!: TemplateRef<{}>;
  @ViewChild('contextMenu', { static: true }) contextMenuTmpl!: TemplateRef<{}>;
  @ViewChild('addDtoDialog', { static: true }) addDtoTmpl!: TemplateRef<{}>;
  @ViewChild('addsServiceDialog', { static: true }) addServiceTmpl!: TemplateRef<{}>;

  newServiceName: string | null = null;
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  webPath: string | null = null;
  previewItem: EntityFile | null = null;
  newDtoFileName: string = '';
  newDtoDescription: string = '';

  selection = new SelectionModel<EntityFile>(true, []);
  selectedWebProjectIds: string[] = [];
  webProjects: SubProjectInfo[] = [];
  currentEntity: EntityFile | null = null;
  currentType: CommandType | null = null;
  isBatch = false;
  isCopied = false;
  isLogin = false;
  isMobile = false;
  showModuleEntity = true;
  config: ProjectConfig | null = null;

  editor: any;
  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public service: EntityInfoService,
    public projectSrv: ProjectService,
    public projectState: ProjectStateService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    private loginService: LoginService
  ) {
    if (projectState.project) {
      this.project = projectState.project;
      this.projectId = projectState.project?.id;
    } else {
      this.projectId = '';
      this.router.navigateByUrl('/');
    }
    this.isLogin = loginService.isLogin;
  }

  ngOnInit(): void {
    this.initForm();
    this.getEntity();
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }
    this.selection.select(...this.dataSource.data);
  }

  toggleModule() {
    this.showModuleEntity = !this.showModuleEntity;
    if (this.showModuleEntity) {
      this.dataSource.data = this.entityFiles;
    } else {
      this.dataSource.data = this.entityFiles.filter(e => e.moduleName === null || e.moduleName === '');
    }

  }
  displayContextMenu(event: any, entity: EntityFile): void {
    if (event.button === 2) {
      this.dialog.closeAll();
      this.dialogRef = this.dialog.open(this.contextMenuTmpl, {
        position: { top: `${event.pageY}px`, left: `${event.pageX}px` },
        disableClose: false,
        data: entity,
      });
    }
  }

  initEditor(editor: any): void {
    this.editor = editor;
  }


  initForm(): void {
    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(null, [Validators.required])
    });
  }

  getEntity(): void {
    this.selection.clear();
    this.service.list(this.projectId!)
      .subscribe(res => {
        if (res.length > 0) {
          this.entityFiles = res;
          this.baseEntityPath = res[0].baseDirPath ?? '';
          this.dataSource = new MatTableDataSource<EntityFile>(this.entityFiles);

          this.dataSource.filterPredicate = (data, filter: string) => {
            if (data.name) {
              return data.name.toLowerCase().indexOf(filter) > -1
                || data.moduleName?.toLowerCase() === filter;
            }
            return false;
          }
        } else {
          this.dataSource = new MatTableDataSource<EntityFile>([]);
        }
        this.isLoading = false;
      })
  }

  clean(): void {
    this.dialogRef = this.dialog.open(ProgressDialogComponent, {
      data: {
        title: '清理中',
        content: '正在清理解决方案，请稍后...'
      }
    })
    this.service.cleanSolution()
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open(res);
            this.getEntity();
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isSync = false;
        },
        complete: () => {
          this.dialogRef.close();
        }
      });
  }

  openRequestDialog(): void {
    this.dialogRef = this.dialog.open(this.requestTmpRef, {
      minWidth: 400
    });
  }
  openSyncDialog(): void {
    this.dialogRef = this.dialog.open(this.syncTmpRef, {
      minWidth: 300
    });
  }
  openNgPagesDialog(element: EntityFile | null): void {
    this.currentEntity = element;
    this.dialogRef = this.dialog.open(this.ngPagesTmpl, {
      minWidth: 400
    });
  }


  openAddDtoDialog(data: EntityFile): void {
    this.previewItem = data;
    this.newDtoFileName = data.name.replace('.cs', '') + 'Dto';
    this.dialog.closeAll();
    this.dialogRef = this.dialog.open(this.addDtoTmpl, {
      minWidth: 400,
    });
  }

  openWithVSCode(data: EntityFile): void {
    window.open(`vscode://file/${data.baseDirPath}${data.baseDirPath}`);
    this.dialogRef.close();
  }

  clearCodesDialog(): void {
    this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: '确认清除？',
        content: '该操作将清除所有选中实体相关生成的代码，部分代码需要您手动删除！'
      }
    });

    this.dialogRef.afterClosed()
      .subscribe({
        next: (res) => {
          if (res) {
            let selected = this.selection.selected;
            if (selected.length > 0) {
              let data: BatchGenerateDto = {
                projectId: this.projectId!,
                entityPaths: selected.map(s => this.baseEntityPath + s.baseDirPath),
                commandType: CommandType.Clear,
                force: this.force
              };
            } else {
              this.snb.open('未选择任何实体');
            }
          }
        }
      });
  }

  openAddEntity(): void {
    this.router.navigateByUrl('/workspace/entity');
  }

  filterEntity() {
    this.dataSource.filter = this.searchKey.trim().toLowerCase();
  }

  searchEntity(event: MatSelectChange) {
    const filterValue = event.value ?? '';
    this.selectedService = filterValue;
    this.getEntity();

  }
  async openPreviewDialog(item: EntityFile, isManager: boolean) {
    if (isManager) {
      item = await this.getFileContent(item.name!, true, item.moduleName ?? '');
    }
    this.previewItem = item;
    this.dialogRef = this.dialog.open(this.previewTmpl, {
      minWidth: 850,
      minHeight: 800
    });
  }

  async getFileContent(entityName: string, isManager: boolean, moduleName: string | null): Promise<EntityFile> {
    return await lastValueFrom(this.service.getFileContent(entityName, isManager, moduleName));
  }

  copyCode(): void {
    this.isCopied = true;
    setTimeout(() => {
      this.isCopied = false;
    }, 1500);
  }

  openInfo(url: string): void {
    window.open(url, '_blank');
  }
  openAddService(): void {
    this.dialogRef = this.dialog.open(this.addServiceTmpl, {
      minWidth: 400
    });
  }

  addService(): void {
    if (this.newServiceName) {
      this.projectSrv.addService(this.newServiceName)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('创建成功');
              this.dialogRef.close();
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isSync = false;
          },
          complete: () => {
            this.isSync = false;
          }
        });

    } else {
      this.snb.open('请输入服务名称');
    }
  }

  openSelectProjectDialog(type: CommandType, element: EntityFile | null): void {
    this.currentEntity = element;
    this.currentType = type;
    switch (type) {
      case CommandType.API:
        this.dialogRef = this.dialog.open(this.apiTmpRef, {
          minWidth: 300
        });
        break;

      case CommandType.Protobuf:
        this.dialogRef = this.dialog.open(this.protobufTmpRef, {
          minWidth: 300
        });
        break;
      default:
        break;
    }
  }

  openGenerateDialog(type: CommandType, element: EntityFile | null): void {
    this.isBatch = element === null;
    this.currentEntity = element;
    this.currentType = type;
    this.dialogRef = this.dialog.open(this.generateTmpRef, {
      minWidth: 300,
    });
  }

  generate(): void {
    if (this.isBatch) {
      this.batch(this.currentType!);
    } else {
      if (this.currentEntity && this.currentType !== null) {
        this.isSync = true;
        const dto: GenerateDto = {
          projectId: this.projectId!,
          entityPath: this.currentEntity.fullName,
          commandType: this.currentType,
          force: this.force
        };
        this.service.generate(dto)
          .subscribe({
            next: (res) => {
              if (res) {
                this.snb.open('生成成功');
                this.dialogRef.close();
                if (this.currentType == CommandType.Dto) {
                  this.currentEntity!.hasDto = true;
                }
                if (this.currentType == CommandType.Manager) {
                  this.currentEntity!.hasManager = true;
                }
              } else {
              }
            },
            error: (error) => {
              this.isSync = false;
              this.snb.open(error.detail);
            },
            complete: () => {
              this.isSync = false;
            }
          });

      } else {
        this.snb.open('未选择任何实体');
      }
    }
  }
  batch(type: CommandType): void {
    let selected = this.selection.selected;
    if (this.currentEntity !== null) {
      selected = [this.currentEntity];
    }
    if (selected.length > 0) {
      let data: BatchGenerateDto = {
        projectId: this.projectId!,
        entityPaths: selected.map(s => s.fullName),
        commandType: type,
        force: this.force
      };
      console.log(data);

      // 参数
      if (this.selectedWebProjectIds.length > 0
        && (type == CommandType.Protobuf
          || type == CommandType.API)) {
        data.projectPath = this.selectedWebProjectIds;
      }

    } else {
      this.snb.open('未选择任何实体');
    }
  }

  selectProject(event: MatSelectionListChange) {
    var data = event.source.selectedOptions.selected;
    this.selectedWebProjectIds = data.map<string>(d => d.value);
  }
  
  goToDto(entity: EntityFile): void {
    this.projectState.currentEntity.set(entity);
    this.router.navigate(['../dto', entity.name], { relativeTo: this.route });
  }
}
