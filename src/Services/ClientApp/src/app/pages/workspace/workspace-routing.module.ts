import { NgModule } from '@angular/core';
import { RouteReuseStrategy, RouterModule, Routes } from '@angular/router';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { IndexComponent } from './index/index.component';
import { NavigationComponent } from './navigation/navigation.component';
import { CustomRouteReuseStrategy } from 'src/app/custom-route-strategy';
import { SettingComponent } from './setting/setting.component';
import { DatabaseComponent } from './database/database.component';
import { EntityComponent } from './entity/entity.component';
import { FeatureComponent } from './feature/feature.component';
import { Json2TypeComponent } from '../tools/json2-type/json2-type.component';
import { TaskComponent } from './task/task.component';
import { StepComponent } from './step/step.component';

const routes: Routes = [
  {
    path: 'workspace',
    component: NavigationComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'index' },
      { path: 'index', component: IndexComponent },
      { path: 'entity', component: EntityComponent },
      { path: 'docs', component: DocsComponent },
      { path: 'database', component: DatabaseComponent },
      { path: 'setting', component: SettingComponent },
      { path: 'dto/:name', component: DtoComponent },
      { path: 'task', component: TaskComponent },
      { path: 'step', component: StepComponent },
      { path: 'feature', component: FeatureComponent },
      { path: 'jsonToType', component: Json2TypeComponent }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
  providers: [{
    provide: RouteReuseStrategy,
    useClass: CustomRouteReuseStrategy
  }]
})
export class WorkspaceRoutingModule { }
