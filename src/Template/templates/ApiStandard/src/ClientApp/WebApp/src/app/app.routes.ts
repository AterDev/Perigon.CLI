import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { LayoutComponent } from './layout/layout.component';
import { NotfoundComponent } from './pages/notfound/notfound.component';
import { AuthGuard } from './share/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      // {
      //   path: 'team',
      //   children: [
      //     { path: '', redirectTo: '/team/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/team/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
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
      {
        path: 'system-role',
        children: [
          { path: '', redirectTo: '/system-role/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-role/index/index.component').then(m => m.IndexComponent) },
        ]
      },
      {
        path: 'system-user',
        children: [
          { path: '', redirectTo: '/system-user/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-user/index/index.component').then(m => m.IndexComponent) },
        ]
      },
      {
        path: 'system-logs',
        children: [
          { path: '', redirectTo: '/system-logs/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-logs/index/index.component').then(m => m.IndexComponent) },
        ]
      },
      // {
      //   path: 'system-config',
      //   children: [
      //     { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-config/index/index.component').then(m => m.IndexComponent) },
      //   ]
      // },
    ],

  },
  { path: '**', component: NotfoundComponent },
];
