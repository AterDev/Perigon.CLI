import { NgModule } from '@angular/core';
import { RouteReuseStrategy, RouterModule, Routes } from '@angular/router';
import { IndexComponent } from './index/index.component';
import { CustomRouteReuseStrategy } from 'src/app/custom-route-strategy';

import { Json2TypeComponent } from './json2-type/json2-type.component';
import { RestfulAPIComponent } from './restful-api/restful-api.component';

const routes: Routes = [
  {
    path: 'tools',
    children: [
      { path: 'index', component: IndexComponent },
      { path: 'jsonToType', component: Json2TypeComponent },
      { path: 'restfulAPI', component: RestfulAPIComponent }

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
export class ToolsRoutingModule { }
