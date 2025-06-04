import { Component, OnInit, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectService } from 'src/app/services/project/project.service';
import { ProjectStateService } from 'src/app/share/project-state.service';

@Component({
    selector: 'app-setting',
    templateUrl: './setting.component.html',
    styleUrls: ['./setting.component.css']
})
export class SettingComponent implements OnInit {
  private service = inject(ProjectService);
  private projectState = inject(ProjectStateService);
  private snb = inject(MatSnackBar);

  isLoading = true;
  projectId: string;
  editorOptions = {
    theme: 'vs-dark', language: 'typescript', minimap: {
      enabled: false
    }
  };

  constructor() {
    const projectState = this.projectState;

    this.projectId = projectState.project!.id;
  }

  ngOnInit(): void {
  }


}
