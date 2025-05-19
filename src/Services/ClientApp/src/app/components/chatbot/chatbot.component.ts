import { HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { AdvanceService } from 'src/app/services/advance/advance.service';


import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
import { NgIf } from '@angular/common';
import { MarkdownComponent } from 'ngx-markdown';
import { MatFormField, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { FormsModule } from '@angular/forms';
import { MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatIcon } from '@angular/material/icon';

export enum ToolType {
  Entity = 0,
  File = 1,
  Answer = 2
}

@Component({
    selector: 'app-chatbot',
    templateUrl: './chatbot.component.html',
    styleUrls: ['./chatbot.component.css'],
    imports: [NgIf, MarkdownComponent, MatFormField, MatInput, CdkTextareaAutosize, FormsModule, MatIconButton, MatSuffix, MatTooltip, MatIcon]
})
export class ChatBotComponent {
  isLoading = true;
  isProcessing = false;
  ToolType = ToolType;
  content: string | null = null;
  selectedTool: ToolType;
  answerContent: string = '本对话由DeepSeek V2模型提供支持!';
  constructor(
    private snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private service: AdvanceService
  ) {
    this.selectedTool = ToolType.Entity;
  }

  ngOnInit() {
  }

  getData(): void { }


  selectTool(event: MatSelectionListChange): void {
    this.selectedTool = event.options[0].value;
  }


  send() {
    if (this.content != null && this.content != "") {
      this.answerContent += `</br>`;
      this.isProcessing = true;

      this.generatorAnswer();
      this.content = null;
    } else {
      this.snb.open('请输入内容');
    }
  }

  clear() {
    this.service.clearChat().subscribe({
      next: (res) => {
        if (res) {
          this.answerContent = '本对话由DeepSeek提供支持!'
        }
      },
      error: (error) => {
        this.snb.open(error.detail);
      },
      complete: () => {
      }
    });;
  }

  generatorAnswer(): void {
    const propmt = encodeURIComponent(this.content || '');
    const url = this.service.baseUrl + `/api/Advance/chat?prompt=${propmt}`;
    const self = this;
    fetch(url, { method: 'GET' }).then((response) => {
      if (!response.ok) {
        this.snb.open('请求失败');
      }
      if (response.body) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        reader.read().then(function processText({ done, value }) {
          if (done) {
            self.isProcessing = false;
            return;
          }
          const res = decoder.decode(value);
          if (res && res != '') {
            self.answerContent += res;
          }
          reader.read().then(processText);
        });
      }
    });
  }

  generateEntity(): void {
    const url = this.service.baseUrl + `/api/Advance/generateEntity?content=${this.content}`;
    const self = this;
    fetch(url, { method: 'POST' }).then((response) => {
      if (!response.ok) {
        this.snb.open('请求失败');
      }
      if (response.body) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        reader.read().then(function processText({ done, value }) {
          if (done) {
            self.isProcessing = false;
            return;
          }
          const res = decoder.decode(value);
          if (res) {
            if (res.includes('\n') || res.includes('\r\n')) {
              self.answerContent += '  ';
            }
            self.answerContent += res;
          }
          reader.read().then(processText);
        });
      }
    });
  }

}
