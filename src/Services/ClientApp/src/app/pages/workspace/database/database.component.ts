import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/services/project/project.service';

import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
import { DomSanitizer } from '@angular/platform-browser';
import { NgIf } from '@angular/common';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MarkdownComponent } from 'ngx-markdown';

@Component({
    selector: 'app-database',
    templateUrl: './database.component.html',
    styleUrls: ['./database.component.css'],
    encapsulation: ViewEncapsulation.None,
    imports: [NgIf, MatProgressSpinner, MarkdownComponent]
})
export class DatabaseComponent implements OnInit {
  isLoding = true;
  content: string | null = null;
  projectId: string | null = null;
  editorOptions = { theme: 'vs-dark', language: 'markdown', minimap: { enabled: false } };
  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private snb: MatSnackBar,
    private sanitizer: DomSanitizer
  ) {
    this.projectId = projectState.project?.id!;
  }

  ngOnInit(): void {
    this.getContent();
  }
  getContent(): void {
    if (this.projectId) { }
  }
}
