import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ToolsService } from 'src/app/services/tools/tools.service';
import { MatButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { EditorComponent } from 'ngx-monaco-editor-v2';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-json2-type',
    templateUrl: './json2-type.component.html',
    styleUrls: ['./json2-type.component.css'],
    imports: [MatButton, MatTooltip, EditorComponent, FormsModule]
})
export class Json2TypeComponent {
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  jsonEditorOptions = { theme: 'vs-dark', language: 'json', minimap: { enabled: false } };
  jsonContent = '';
  classCode: string | null = null;
  editor: any;

  constructor(
    private service: ToolsService,
    private snb: MatSnackBar
  ) {
  }

  onInit(editor: any) {
    this.editor = editor;
  }

  ngOnInit(): void {

  }

  convert(): void {
    if (!this.jsonContent) {
      this.snb.open('请输入json内容', '关闭', { duration: 2000 });
      return;
    }

    this.service.convertToClass({
      content: this.jsonContent
    }).subscribe(res => {
      this.classCode = res.join('\n');
    });
  }
}
