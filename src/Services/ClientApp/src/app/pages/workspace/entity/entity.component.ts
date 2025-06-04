import { Component, inject } from '@angular/core';
import { Location } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AdvanceService } from 'src/app/services/advance/advance.service';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { MatToolbar } from '@angular/material/toolbar';
import { MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { EditorComponent } from 'ngx-monaco-editor-v2';

@Component({
    selector: 'app-entity',
    templateUrl: './entity.component.html',
    styleUrls: ['./entity.component.css'],
    imports: [MatToolbar, MatIconButton, MatTooltip, MatIcon, MatFormField, MatLabel, MatInput, FormsModule, MatProgressSpinner, EditorComponent]
})
export class EntityComponent {
  snb = inject(MatSnackBar);
  router = inject(Router);
  service = inject(AdvanceService);
  projectState = inject(ProjectStateService);
  private location = inject(Location);

  isProcessing = false;
  name: string | null = null;
  description: string | null = null;
  entities: string[] | null = null;
  selectedIndex: number | null = null;
  selectedContent: string | null = null;
  namespace: string | null = null;
  projectId: string | null = null;
  editorOptions = {
    theme: 'vs-dark', language: 'csharp', minimap: {
      enabled: false
    }
  };
  constructor() {
    const projectState = this.projectState;

    if (projectState.project)
      this.projectId = projectState.project?.id;
  }
  ngOnInit(): void {

  }

  onInit(editor: any) {

  }


  back(): void {
    this.location.back();

  }
}
