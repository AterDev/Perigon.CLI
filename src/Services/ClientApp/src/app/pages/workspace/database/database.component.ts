import { Component, OnInit, ViewEncapsulation, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/services/project/project.service';

import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
import { DomSanitizer } from '@angular/platform-browser';

import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MarkdownComponent } from 'ngx-markdown';

@Component({
    selector: 'app-database',
    templateUrl: './database.component.html',
    styleUrls: ['./database.component.css'],
    encapsulation: ViewEncapsulation.None,
    imports: [MatProgressSpinner, MarkdownComponent]
})
export class DatabaseComponent implements OnInit {
  private service = inject(ProjectService);
  private projectState = inject(ProjectStateService);
  private snb = inject(MatSnackBar);
  private sanitizer = inject(DomSanitizer);

  isLoding = true;
  content: string | null = null;
  projectId: string | null = null;
  editorOptions = { theme: 'vs-dark', language: 'markdown', minimap: { enabled: false } };
  constructor() {
    const projectState = this.projectState;

    this.projectId = projectState.project?.id!;
  }

  ngOnInit(): void {
    this.getContent();
  }
  getContent(): void {
    if (this.projectId) { }
  }
}
