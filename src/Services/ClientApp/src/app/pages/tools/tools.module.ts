import { NgModule } from '@angular/core';
import { IndexComponent } from './index/index.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { ShareModule } from 'src/app/share/share.module';

import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { MarkdownModule } from 'ngx-markdown';
import { ToolsRoutingModule } from './tools-routing.module';
import { Json2TypeComponent } from './json2-type/json2-type.component';
import { RestfulAPIComponent } from './restful-api/restful-api.component';

@NgModule({
    imports: [
        ComponentsModule,
        ShareModule,
        ToolsRoutingModule,
        MonacoEditorModule,
        MarkdownModule.forRoot(),
        IndexComponent,
        Json2TypeComponent,
        RestfulAPIComponent
    ]
})
export class ToolsModule { }
