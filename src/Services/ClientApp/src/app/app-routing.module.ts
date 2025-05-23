import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IndexComponent } from './pages/tools/index/index.component';
import { Json2TypeComponent } from './pages/tools/json2-type/json2-type.component';
import { RestfulAPIComponent } from './pages/tools/restful-api/restful-api.component';

const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', redirectTo: 'index', pathMatch: 'full' },
  { path: 'login', redirectTo: 'login', pathMatch: 'full' },
  { path: 'ai', redirectTo: 'ai/index', pathMatch: 'full' },
  { path: 'tools', redirectTo: 'tools/index', pathMatch: 'full' },
  { path: '*', redirectTo: '', pathMatch: 'full' },

  {
    path: 'tools', children: [
      { path: 'index', component: IndexComponent },
      { path: 'json2Type', component: Json2TypeComponent },
    ]
  }

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }