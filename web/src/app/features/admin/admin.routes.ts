import { Routes } from '@angular/router';

import { adminGuard } from '../../core/auth/admin.guard';
import { adminLoginGuard } from '../../core/auth/admin-login.guard';

export const adminRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    canActivate: [adminLoginGuard],
    loadComponent: () =>
      import('./login/admin-login.component').then((m) => m.AdminLoginComponent)
  },
  {
    path: '',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent)
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./products/admin-products.component').then((m) => m.AdminProductsComponent)
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./orders/admin-orders.component').then((m) => m.AdminOrdersComponent)
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./users/admin-users.component').then((m) => m.AdminUsersComponent)
      },
      {
        path: 'admins',
        loadComponent: () =>
          import('./admins/admin-admins.component').then((m) => m.AdminAdminsComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
