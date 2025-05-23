import { Component } from '@angular/core';
import { Location, NgIf } from '@angular/common';
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
    imports: [MatToolbar, MatIconButton, MatTooltip, MatIcon, MatFormField, MatLabel, MatInput, FormsModule, NgIf, MatProgressSpinner, EditorComponent]
})
export class EntityComponent {
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
  constructor(
    public snb: MatSnackBar,
    public router: Router,
    public service: AdvanceService,
    public projectState: ProjectStateService,
    private location: Location
  ) {
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
