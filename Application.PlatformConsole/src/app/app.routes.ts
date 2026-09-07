import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const APP_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', loadComponent: () => import('./pages/login.component').then((m) => m.LoginComponent) },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard.component').then((m) => m.DashboardComponent) },
      { path: 'tenants', loadComponent: () => import('./pages/tenants.component').then((m) => m.TenantsComponent) },
      { path: 'tenants/:id', loadComponent: () => import('./pages/tenant-detail.component').then((m) => m.TenantDetailComponent) },
      { path: 'plans', loadComponent: () => import('./pages/plans.component').then((m) => m.PlansComponent) },
      { path: 'modules', loadComponent: () => import('./pages/modules.component').then((m) => m.ModulesComponent) },
      { path: 'audit', loadComponent: () => import('./pages/audit.component').then((m) => m.AuditComponent) },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
