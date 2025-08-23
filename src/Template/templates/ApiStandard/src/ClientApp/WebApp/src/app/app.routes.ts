import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { LayoutComponent } from './layout/layout.component';
import { Notfound } from './pages/notfound/notfound';
import { AuthGuard } from './share/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
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
      //     { path: 'index', loadComponent: () => import('./pages/team/index/index').then(m => m.Index) },
      //   ]
      // },
      // {
      //   path: 'product',
      //   children: [
      //     { path: '', redirectTo: '/product/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/product/index/index').then(m => m.Index) },
      //   ]
      // },
      // {
      //   path: 'order',
      //   children: [
      //     { path: '', redirectTo: '/order/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/order/index/index').then(m => m.Index) },
      //   ]
      // },
      {
        path: 'system-role',
        children: [
          { path: '', redirectTo: '/system-role/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-role/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-user',
        children: [
          { path: '', redirectTo: '/system-user/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-user/index/index').then(m => m.Index) },
        ]
      },
      {
        path: 'system-logs',
        children: [
          { path: '', redirectTo: '/system-logs/index', pathMatch: 'full' },
          { path: 'index', loadComponent: () => import('./pages/system-logs/index/index').then(m => m.Index) },
        ]
      },
      // {
      //   path: 'system-config',
      //   children: [
      //     { path: '', redirectTo: '/system-config/index', pathMatch: 'full' },
      //     { path: 'index', loadComponent: () => import('./pages/system-config/index/index').then(m => m.Index) },
      //   ]
      // },
    ],

  },
  { path: '**', component: Notfound },
];
