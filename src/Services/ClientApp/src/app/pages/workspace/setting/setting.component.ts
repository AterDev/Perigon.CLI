import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectService } from 'src/app/services/project/project.service';
import { ProjectStateService } from 'src/app/share/project-state.service';

@Component({
    selector: 'app-setting',
    templateUrl: './setting.component.html',
    styleUrls: ['./setting.component.css']
})
export class SettingComponent implements OnInit {
  isLoading = true;
  projectId: string;
  editorOptions = {
    theme: 'vs-dark', language: 'typescript', minimap: {
      enabled: false
    }
  };

  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private snb: MatSnackBar
  ) {
    this.projectId = projectState.project!.id;
  }

  ngOnInit(): void {
  }


}
