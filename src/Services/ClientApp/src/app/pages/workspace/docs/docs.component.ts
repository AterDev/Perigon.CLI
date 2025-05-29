import { Component, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogRef, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ApiDocInfoItemDto } from 'src/app/services/api-doc-info/models/api-doc-info-item-dto.model';
import { ApiDocInfo } from 'src/app/services/api-doc-info/models/api-doc-info.model';
import { ApiDocTag } from 'src/app/services/models/api-doc-tag.model';
import { ComponentType } from 'src/app/services/enum/models/component-type.model';
import { OperationType } from 'src/app/services/enum/models/operation-type.model';
import { RequestLibType } from 'src/app/services/enum/models/request-lib-type.model';
import { Project } from 'src/app/services/project/models/project.model';
import { PropertyInfo } from 'src/app/services/models/property-info.model';
import { RestApiGroup } from 'src/app/services/models/rest-api-group.model';
import { RestApiInfo } from 'src/app/services/models/rest-api-info.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ApiDocInfoService } from 'src/app/services/api-doc-info/api-doc-info.service';
import { EntityInfoService } from 'src/app/services/entity-info/entity-info.service';
import { ProjectService } from 'src/app/services/project/project.service';
import { ModelInfo } from 'src/app/services/models/model-info.model';
import { GenActionService } from 'src/app/services/gen-action/gen-action.service';
import { GenSourceType } from 'src/app/services/enum/models/gen-source-type.model';
import { GenActionItemDto } from 'src/app/services/gen-action/models/gen-action-item-dto.model';
import { GenActionRunDto } from 'src/app/services/gen-action/models/gen-action-run-dto.model';
import { ModelFileItemDto } from 'src/app/services/gen-action/models/model-file-item-dto.model';
import { NgIf, NgFor } from '@angular/common';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatToolbar } from '@angular/material/toolbar';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatSelect } from '@angular/material/select';
import { MatOption } from '@angular/material/core';
import { MatIconButton, MatButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatTabGroup, MatTab } from '@angular/material/tabs';
import { MatInput } from '@angular/material/input';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle } from '@angular/material/expansion';
import { MatNavList, MatListItem } from '@angular/material/list';
import { MatChipSet, MatChip } from '@angular/material/chips';
import { MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow } from '@angular/material/table';
import { TypedCellDefDirective } from '../../../components/typed-cell-def.directive';
import { EditorComponent } from 'ngx-monaco-editor-v2';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { TypeMeta } from 'src/app/services/models/type-meta.model';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css'],
  imports: [NgIf, MatProgressSpinner, MatToolbar, MatFormField, MatSelect, FormsModule, NgFor, MatOption, MatIconButton, MatTooltip, MatIcon, MatTabGroup, MatTab, MatInput, MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle, MatNavList, MatListItem, MatChipSet, MatChip, MatTable, MatColumnDef, MatHeaderCellDef, MatHeaderCell, TypedCellDefDirective, MatCellDef, MatCell, MatHeaderRowDef, MatHeaderRow, MatRowDef, MatRow, MatLabel, MatButton, EditorComponent, MatDialogTitle, CdkScrollable, MatDialogContent, ReactiveFormsModule, MatDialogActions, MatDialogClose]
})
export class DocsComponent implements OnInit {
  projectSrv = inject(ProjectService);
  projectState = inject(ProjectStateService);
  service = inject(ApiDocInfoService);
  private genActionService = inject(GenActionService);
  entitySrv = inject(EntityInfoService);
  router = inject(Router);
  dialog = inject(MatDialog);
  snb = inject(MatSnackBar);

  OperationType = OperationType;
  RequestLibType = RequestLibType;
  ComponentType = ComponentType;
  project = {} as Project;
  projectId: string;
  isRefresh = false;
  isSync = false;
  isLoading = true;
  isOccupying = false;
  currentApi: RestApiInfo | null = null;
  currentModel: TypeMeta | null = null;
  selectedModel: TypeMeta | null = null;
  searchKey: string | null = null;
  modelSearchKey: string | null = null;
  /**
   * 文档列表
   */
  docs = [] as ApiDocInfoItemDto[];

  actions = [] as GenActionItemDto[];
  selectedActionId: string | null = null;
  currentDoc: ApiDocInfoItemDto | null = null;
  newDoc = {} as ApiDocInfo;
  addForm!: FormGroup;
  editForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  requestForm!: FormGroup;
  clientRequestForm!: FormGroup;
  outputFiles: ModelFileItemDto[] = [];

  @ViewChild("addDocDialog", { static: true })
  addTmpRef!: TemplateRef<{}>;

  @ViewChild("editDocDialog", { static: true })
  editTmpRef!: TemplateRef<{}>;

  @ViewChild("modelInfo", { static: true })
  modelTmpRef!: TemplateRef<{}>;

  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;

  @ViewChild("clientRequestDialog", { static: true })
  clientRequestTmpRef!: TemplateRef<{}>;
  restApiGroups = [] as RestApiGroup[];
  filterApiGroups = [] as RestApiGroup[];
  filterModelInfos = [] as TypeMeta[];
  modelInfos = [] as TypeMeta[];
  tags = [] as ApiDocTag[];
  tableColumns = ['name', 'type', 'requried', 'description'];
  modelTableColumns = ['name', 'type', 'requried', 'description', 'validator'];
  editorHtmlOptions = {
    theme: 'vs-dark', language: 'html', minimap: {
      enabled: false
    }
  };
  editorTSOptions = {
    theme: 'vs-dark', language: 'typescript', minimap: {
      enabled: false
    }
  };

  constructor() {
    const projectState = this.projectState;

    if (projectState.project) {
      this.projectId = projectState.project.id;
      this.project = projectState.project;
    } else {
      this.projectId = '';
      this.router.navigateByUrl('/');
    }
  }

  ngOnInit(): void {
    this.getDocs();
    this.getGenActions();
    this.initForm();
  }

  initForm(): void {
    // 添加表单
    this.addForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>('OpenAPI', [Validators.maxLength(100)]),
      path: new FormControl<string | null>('http://localhost:5002/swagger/name/swagger.json', [Validators.required, Validators.maxLength(200)]),
    });

    // 更新表单
    this.editForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>(null, [Validators.maxLength(100)]),
      path: new FormControl<string | null>('http://localhost:5002/swagger/name/swagger.json', [Validators.required, Validators.maxLength(200)]),
    });

    // 生成请求表单
    let defaultPath = this.project.path + '\\src\\ClientApp\\src\\app';

    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(defaultPath, [Validators.required])
    });

    this.clientRequestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl(0, []),
      path: new FormControl<string | null>(this.project.path + "\\src\\SDK\\", [Validators.required])
    });
  }

  getDocs(): void {
    this.service.list()
      .subscribe({
        next: (res) => {
          if (res) {
            this.docs = res;
            if (res.length > 0) {
              this.currentDoc = res[0];
              this.getDocContent(false);
            } else {
              this.isLoading = false;
            }
          }

        },
        error: error => {
          this.isLoading = false;
          this.snb.open(error);
        }
      });
  }

  getGenActions(): void {
    this.genActionService.filter({
      sourceType: GenSourceType.DtoModel,
      pageIndex: 1,
      pageSize: 99
    }).subscribe({
      next: (res) => {
        if (res.data) {
          this.actions = res.data;
        }
      },
      error: (error) => {
        this.snb.open(error.detail);
      },
      complete: () => {
      }
    });
  }
  export(): void {
    this.isSync = true;
    this.service.export(this.currentDoc!.id)
      .subscribe({
        next: (res) => {
          this.service.openFile(res, `${this.currentDoc?.name}.md`);
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isSync = false;
        },
        complete: () => {
          this.isSync = false;
        }
      });
  }

  openAddDocDialog(): void {
    this.dialogRef = this.dialog.open(this.addTmpRef, {
      minWidth: 400
    });
  }

  openEditDocDialog(): void {
    this.editForm.get('name')?.setValue(this.currentDoc?.name);
    this.editForm.get('description')?.setValue(this.currentDoc?.description);
    this.editForm.get('path')?.setValue(this.currentDoc?.path);
    this.dialogRef = this.dialog.open(this.editTmpRef, {
      minWidth: 400
    });
  }

  addDoc(): void {
    if (this.addForm.valid) {
      const data = this.addForm.value as ApiDocInfo;
      data.projectId = this.projectId;
      this.service.add(data)
        .subscribe(res => {
          if (res) {
            this.snb.open('添加成功');
            this.isLoading = true;
            this.getDocs();
            this.addForm.reset();
            this.addForm.get('path')?.setValue('http://localhost:5002/swagger/name/swagger.json');
            this.dialogRef.close();
          }
        });
    }
  }
  openRequestDialog(): void {
    this.requestForm.get('swagger')?.setValue(this.currentDoc?.path);
    if (this.currentDoc?.localPath) {
      this.requestForm.get('path')?.setValue(this.currentDoc?.localPath);
    }
    this.dialogRef = this.dialog.open(this.requestTmpRef, {
      minWidth: 400
    });
  }

  openClientRequestDialog(): void {
    this.clientRequestForm.get('swagger')?.setValue(this.currentDoc?.path);
    this.clientRequestForm.get('path')?.setValue(this.project.path + "\\src\\SDK\\");
    this.dialogRef = this.dialog.open(this.clientRequestTmpRef, {
      minWidth: 400
    });
  }

  delete(): void {
    const id = this.currentDoc!.id;
    if (id) {
      this.service.delete(id)
        .subscribe(res => {
          if (res) {
            this.snb.open('删除成功');
            this.getDocs();
          }
        })
    }
  }

  editDoc(): void {
    if (this.editForm.valid) {
      const data = this.editForm.value as ApiDocInfo;
      data.projectId = this.projectId;
      if (this.currentDoc != null && this.currentDoc.id) {
        this.service.update(this.currentDoc.id, data)
          .subscribe(res => {
            if (res) {
              this.snb.open('更新成功');
              this.currentDoc!.name = data.name;
              this.currentDoc!.description = data.description;
              this.currentDoc!.path = data.path;
              this.editForm.reset();
              this.dialogRef.close();
            }
          });
      } else {
        this.snb.open('未选择接口文档');
      }
    }
  }

  generateRequest(): void {
    this.isSync = true;
    const swagger = this.requestForm.get('swagger')?.value as string;
    const type = this.requestForm.get('type')?.value as number;
    const path = this.requestForm.get('path')?.value as string;
    this.service.generateRequest(this.currentDoc?.id!, path, type, swagger)
      .subscribe({
        next: res => {
          if (res) {
            this.snb.open('生成成功');
            this.dialogRef.close();
          }
          this.isSync = false;
        },
        error: () => {
          this.isSync = false;
        }
      })
  }

  generateClientRequest(): void {
    this.isSync = true;
    const swagger = this.clientRequestForm.get('swagger')?.value as string;
    const type = this.clientRequestForm.get('type')?.value as number;
    const path = this.clientRequestForm.get('path')?.value as string;
    this.service.generateCsharpRequest(this.currentDoc?.id!, path, swagger)
      .subscribe({
        next: res => {
          if (res) {
            this.snb.open('生成成功');
            this.dialogRef.close();
          }
          this.isSync = false;
        },
        error: () => {
          this.isSync = false;
        }
      })

  }


  runGenAction(): void {
    if (this.currentModel && this.selectedActionId) {
      this.isSync = true;
      const data: GenActionRunDto = {
        id: this.selectedActionId,
        onlyOutput: true,
        modelInfo: this.currentModel
      }
      this.genActionService.execute(data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('生成成功');
              if (res.outputFiles) {
                this.outputFiles = res.outputFiles;
              }
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

  getDocContent(isFresh: boolean = true): void {
    const id = this.currentDoc!.id;
    if (id) {
      this.service.getApiDocContent(id, isFresh)
        .subscribe(
          {
            next: res => {
              if (res) {
                this.restApiGroups = res.restApiGroups!;
                this.filterApiGroups = this.restApiGroups;
                this.modelInfos = res.typeMeta!;
                this.filterModelInfos = this.modelInfos.filter(m => m.isEnum == false);
                this.tags = res.openApiTags!;
                // 更新当前展示的内容
                if (this.currentApi != null) {
                  const updateContent = this.filterApiGroups
                    .map(g => g.apiInfos!)
                    .flat(1)
                    .find((a) => a?.router == this.currentApi?.router);

                  if (updateContent) {
                    this.currentApi = updateContent;
                  }
                }
              }
              this.isLoading = false;
            },
            error: error => {
              this.isLoading = false;
            }
          })
    }
  }

  filterApis(): void {
    if (this.searchKey && this.searchKey != null) {
      const searchKey = this.searchKey.toLowerCase();
      this.filterApiGroups = this.restApiGroups.filter((val) => {
        return val.name?.toLowerCase().includes(searchKey)
          || val.apiInfos!.findIndex((api) => {
            return api.router?.toLowerCase().includes(searchKey)
              || api.summary?.toLowerCase().includes(searchKey)
              || api.tag?.toLowerCase().includes(searchKey)
          }) > -1
      });

      for (let index = 0; index < this.filterApiGroups.length; index++) {
        const group = this.filterApiGroups[index];
        this.filterApiGroups[index].apiInfos = group.apiInfos!
          .filter((api) => {
            return api.router?.toLowerCase().includes(searchKey)
              || api.summary?.toLowerCase().includes(searchKey)
              || api.tag?.toLowerCase().includes(searchKey)
          })
      }
    } else {
      this.filterApiGroups = this.restApiGroups;
    }
  }

  filterModels(): void {
    if (this.modelSearchKey && this.modelSearchKey != null) {
      const modelSearchKey = this.modelSearchKey.toLowerCase();
      this.filterModelInfos = this.modelInfos.filter((val) => {
        return val.name?.toLowerCase().includes(modelSearchKey)
          || val.comment?.toLowerCase().includes(modelSearchKey)
          && val.isEnum == false
      });
    } else {
      this.filterModelInfos = this.modelInfos;
    }
  }

  selectApi(api: RestApiInfo): void {
    this.currentApi = api;
  }
  selectModel(model: TypeMeta): void {
    this.currentModel = model;
  }

  showModel(prop: PropertyInfo): void {
    if (prop.isNavigation) {
      this.selectedModel = this.modelInfos.find(m => m.name == prop.navigationName) ?? null;
      console.log(this.selectedModel);
      if (this.selectedModel) {
        this.dialog.closeAll();
        this.dialog.open(this.modelTmpRef, {
          minWidth: 400
        });
      }
    }
  }

  getApiTip(api: RestApiInfo): string {
    return `[${OperationType[api.operationType!]}] ${api.router}`;
  }

  getApiTypeColor(type: OperationType): string {
    switch (type) {
      case OperationType.Get:
        return '#318deb';
      case OperationType.Post:
        return '#14cc78';
      case OperationType.Put:
        return '#fca130';
      case OperationType.Patch:
        return '#fca130';
      case OperationType.Delete:
        return '#f93e3e';
      default:
        return '#888888';
    }
  }

  refresh(): void {
    this.isLoading = true;
    this.getDocContent(true);
  }
}
