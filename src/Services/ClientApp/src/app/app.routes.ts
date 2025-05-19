import { Routes } from '@angular/router';
import { AuthGuard } from './auth/auth.guard';
import { LayoutComponent } from './components/layout/layout.component';
import { NavigationComponent } from './pages/workspace/navigation/navigation.component';

export const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  {
    path: 'index', loadComponent: () => import('./pages/home/index/index.component')
      .then(m => m.IndexComponent)
  },
  {
    path: 'create', loadComponent: () => import('./pages/home/create/create.component')
      .then(m => m.CreateComponent)
  },
  { path: 'tools', pathMatch: 'full', redirectTo: 'tools/index' },


  {
    path: 'tools',
    children: [
      {
        path: 'index', loadComponent: () => import('./pages/tools/index/index.component')
          .then(m => m.IndexComponent)
      },
      {
        path: 'json2Type', loadComponent: () => import('./pages/tools/json2-type/json2-type.component')
          .then(m => m.Json2TypeComponent)
      },
      {
        path: 'restfulAPI', loadComponent: () => import('./pages/tools/restful-api/restful-api.component')
          .then(m => m.RestfulAPIComponent)
      }
    ]
  },
  {
    path: 'workspace',
    component: NavigationComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'index' },
      { path: 'index', loadComponent: () => import('./pages/workspace/index/index.component').then(m => m.IndexComponent) },
      { path: 'entity', loadComponent: () => import('./pages/workspace/entity/entity.component').then(m => m.EntityComponent) },
      { path: 'docs', loadComponent: () => import('./pages/workspace/docs/docs.component').then(m => m.DocsComponent) },
      { path: 'database', loadComponent: () => import('./pages/workspace/database/database.component').then(m => m.DatabaseComponent) },
      { path: 'setting', loadComponent: () => import('./pages/workspace/setting/setting.component').then(m => m.SettingComponent) },
      { path: 'dto/:name', loadComponent: () => import('./pages/workspace/dto/dto.component').then(m => m.DtoComponent) },
      { path: 'task', loadComponent: () => import('./pages/workspace/task/task.component').then(m => m.TaskComponent) },
      { path: 'step', loadComponent: () => import('./pages/workspace/step/step.component').then(m => m.StepComponent) },
      { path: 'feature', loadComponent: () => import('./pages/workspace/feature/feature.component').then(m => m.FeatureComponent) },
      { path: 'jsonToType', loadComponent: () => import('./pages/tools/json2-type/json2-type.component').then(m => m.Json2TypeComponent) }
      // {
      //   path: 'product',
      //   children: [
      //     { path: '', redirectTo: '/product/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/product/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
      // {
      //   path: 'order',
      //   children: [
      //     { path: '', redirectTo: '/order/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/order/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
      // {
      //   path: 'system-role',
      //   children: [
      //     { path: '', redirectTo: '/system-role/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-role/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
      // {
      //   path: 'system-user',
      //   children: [
      //     { path: '', redirectTo: '/system-user/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-user/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
      // {
      //   path: 'system-logs',
      //   children: [
      //     { path: '', redirectTo: '/system-logs/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-logs/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
      // {
      //   path: 'system-config',
      //   children: [
      //     { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-config/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
    ],

  },
];
