import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogRef, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Route, Router, RouterLink } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { SolutionType } from 'src/app/services/enum/models/solution-type.model';
import { Project } from 'src/app/services/project/models/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/services/project/project.service';

import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
import 'prismjs/components/prism-csharp.min.js';
import { AdvanceService } from 'src/app/services/advance/advance.service';
import { ControllerType } from 'src/app/services/enum/models/controller-type.model';
import { ProjectConfig } from 'src/app/services/project/models/project-config.model';
import { NgIf, NgFor } from '@angular/common';
import { MatButton, MatMiniFabAnchor, MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions } from '@angular/material/card';
import { CdkScrollable } from '@angular/cdk/scrolling';
import { MatFormField, MatLabel, MatHint } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatRadioGroup, MatRadioButton } from '@angular/material/radio';
import { MatProgressBar } from '@angular/material/progress-bar';
import { MarkdownComponent } from 'ngx-markdown';
import { EnumTextPipe } from '../../../pipe/enum-text.pipe';
import { ToKeyValuePipe } from '../../../share/pipe/to-key-value.pipe';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css'],
  imports: [NgIf, MatButton, MatTooltip, MatMiniFabAnchor, RouterLink, MatIcon, NgFor, MatCard, MatCardHeader, MatCardTitle, MatIconButton, MatCardSubtitle, MatCardContent, MatCardActions, MatDialogTitle, CdkScrollable, MatDialogContent, FormsModule, ReactiveFormsModule, MatFormField, MatLabel, MatInput, MatDialogActions, MatDialogClose, MatHint, MatRadioGroup, MatRadioButton, MatProgressBar, MarkdownComponent, EnumTextPipe, ToKeyValuePipe]
})
export class IndexComponent implements OnInit {
  @ViewChild("addDialog", { static: true }) dialogTmpRef!: TemplateRef<{}>;
  @ViewChild("settingDialog", { static: true }) settingTmpRef!: TemplateRef<{}>;
  @ViewChild("updateProjectDialog", { static: true }) updateTmpRef!: TemplateRef<{}>;
  @ViewChild("addOpenAiKeyDialog", { static: true }) openAITmpRef!: TemplateRef<{}>;
  SolutionType = SolutionType;
  ControllerType = ControllerType;
  dialogRef!: MatDialogRef<{}, any>;
  projects = [] as Project[];
  current: Project | null = null;
  config: ProjectConfig | null = null;
  addForm!: FormGroup;
  settingForm!: FormGroup;
  type: string | null = null;
  isLoading = true;
  isProcessing = false;
  isUpdating = false;
  updated = false;
  updateResult: string | null = null;
  version: string | null;
  deepSeekApiKey: string | null = null;

  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private advance: AdvanceService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    public router: Router,
    public route: ActivatedRoute
  ) {
    this.type = localStorage.getItem('type');
    this.version = projectState.version;
  }

  ngOnInit(): void {
    this.getProjects();
    this.initForm();
  }

  initForm(): void {
    this.addForm = new FormGroup({
      displayName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]),
      path: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(100)])
    })
  }

  needUpdate(currentVersion: string | null): boolean {
    const majorVersion = this.version?.split('.')[0];
    const currentMajorVersion = currentVersion?.split('.')[0];
    if (majorVersion == null || currentMajorVersion == null) return false;
    return majorVersion !== currentMajorVersion;
  }

  getOpenAIKey(): void {
    this.advance.getConfig("deepSeekApiKey")
      .subscribe({
        next: (res) => {
          if (res) {
            this.deepSeekApiKey = res.value;
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  addProjectDialog(): void {
    this.dialogRef = this.dialog.open(this.dialogTmpRef, {
      minWidth: 360
    });
  }

  addProject(): void {
    if (this.addForm.valid) {
      const name = this.addForm.get('displayName')?.value as string;
      const path = this.addForm.get('path')?.value as string;
      this.service.add(name, path)
        .subscribe({
          next: (res) => {
            this.snb.open('添加成功');
            this.dialogRef.close();
            this.addForm.reset();
            this.getProjects();
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    } else {
      this.snb.open('输入有误，请查检');
    }
  }

  openSolution(path: string): void {
    this.service.openSolution(path)
      .subscribe({
        next: (res) => {
          if (res) {
          } else {
            this.snb.open('打开失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  openSetting(project: Project): void {
    this.current = project;
    this.projectState.setProject(project);
    this.isProcessing = true;
    this.config = project.config!;
    this.buildSettingForm();
    this.dialogRef = this.dialog.open(this.settingTmpRef, {
      minWidth: 450,
    })
  }

  openUpdate(project: Project): void {
    this.current = project;
    this.projectState.setProject(project);
    this.dialogRef = this.dialog.open(this.updateTmpRef, {
      minWidth: 450,
      hasBackdrop: true,
      closeOnNavigation: false,
      disableClose: true
    })
  }

  openOpenAI(): void {
    this.dialogRef = this.dialog.open(this.openAITmpRef, {
      minWidth: 450,
      hasBackdrop: true,
      closeOnNavigation: false,
    })
  }

  buildSettingForm(): void {
    this.settingForm = new FormGroup({
      sharePath: new FormControl(this.config?.sharePath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      entityPath: new FormControl(this.config?.entityPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      entityFrameworkPath: new FormControl(this.config?.entityFrameworkPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      applicationPath: new FormControl(this.config?.applicationPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      apiPath: new FormControl(this.config?.apiPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      idType: new FormControl(this.config?.idType ?? "Guid", [Validators.required, Validators.minLength(2), Validators.maxLength(20)]),
      controllerType: new FormControl(this.config?.controllerType ?? 0, [Validators.required]),
    });
  }

  saveSetting(): void {
    if (this.settingForm.valid && this.current) {
      const idType = this.settingForm.get('idType')?.value;
      if (idType !== 'Guid' && idType !== 'int') {
        this.snb.open('Id类型仅支持Guid或int');
        return;
      }
      this.isProcessing = true;
      const data = this.settingForm.value as ProjectConfig;
      console.log(data);

      this.service.updateConfig(this.current.id, data)
        .subscribe({
          next: (res) => {
            this.snb.open('保存成功');
            this.dialogRef.close();
            this.getProjects();
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
            this.isProcessing = false;
          }
        });
    } else {
      console.log(this.settingForm.errors);
    }
  }

  addOpenAiKey(): void {
    if (this.deepSeekApiKey) {
      this.advance.setConfig("deepSeekApiKey", this.deepSeekApiKey)
        .subscribe({
          next: (res) => {
            this.snb.open('保存成功');
            this.dialogRef.close();
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    }
  }

  deleteConfirm(item: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      minWidth: 300,
      data: {
        title: '删除项目',
        content: '将项目从工具中删除(不会删除本地文件)，是否确定?'
      }
    });

    ref.afterClosed().subscribe(res => {
      if (res) {
        this.delete(item);
      }
    });
  }

  delete(id: string): void {
    if (id) {
      this.service.delete(id)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('删除成功');
              this.getProjects();
            } else {
              this.snb.open('删除失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        })
    }
  }

  updateProject(): void {
    this.isUpdating = true;
    this.updated = false;

    this.service.updateSolution()
      .subscribe({
        next: (res) => {
          this.isUpdating = false;
          this.updated = true;
          this.updateResult = res;
          this.dialogRef.afterClosed().subscribe(_ => {
            this.updated = false;
            this.getProjects();
          });
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isUpdating = false;
          this.updated = true;
        },
        complete: () => {
          this.isUpdating = false;
          this.updated = true;
        }
      });
  }
  selectProject(id: string): void {
    const project = this.projects.find(p => p.id == id);
    if (project) {
      this.projectState.setProject(project);
      this.router.navigateByUrl('/workspace/index');

    } else {
      this.snb.open('无效的项目');
    }
  }
  getProjects(): void {
    this.isLoading = true;
    this.service.list()
      .subscribe({
        next: (res) => {
          if (res) {
            this.projects = res;
          } else {
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }
}
