import { NgModule } from '@angular/core';
import { WorkspaceRoutingModule } from './workspace-routing.module';
import { IndexComponent } from './index/index.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { ShareModule } from 'src/app/share/share.module';
import { NavigationComponent } from './navigation/navigation.component';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { MarkdownModule } from 'ngx-markdown';
import { SettingComponent } from './setting/setting.component';
import { DatabaseComponent } from './database/database.component';
import { EntityComponent } from './entity/entity.component';
import { FeatureComponent } from './feature/feature.component';

import { TaskComponent } from './task/task.component';
import { StepComponent } from './step/step.component';
import { DragDropModule } from "@angular/cdk/drag-drop";
import { AsyncPipe } from '@angular/common';
import { EnumTextPipe } from 'src/app/pipe/enum-text.pipe';

@NgModule({
    imports: [
    ComponentsModule,
    ShareModule,
    WorkspaceRoutingModule,
    MonacoEditorModule,
    MarkdownModule.forRoot(),
    DragDropModule,
    AsyncPipe,
    EnumTextPipe,
    IndexComponent,
    NavigationComponent,
    DocsComponent,
    DtoComponent,
    SettingComponent,
    DatabaseComponent,
    EntityComponent,
    FeatureComponent,
    TaskComponent,
    StepComponent
]
})
export class WorkspaceModule { }
