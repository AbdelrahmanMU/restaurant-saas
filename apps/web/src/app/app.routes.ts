import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'activate',
    loadComponent: () => import('./pages/activate/activate.component').then(m => m.ActivateComponent)
  },
  {
    path: 'cashier',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/cashier/cashier.component').then(m => m.CashierComponent)
  },
  {
    path: 'coordinator',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/coordinator/coordinator.component').then(m => m.CoordinatorComponent)
  },
  {
    path: 'branch-manager',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/branch-manager/branch-manager.component').then(m => m.BranchManagerComponent)
  },
  {
    path: 'restaurant-manager',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/restaurant-manager/restaurant-manager.component').then(m => m.RestaurantManagerComponent)
  },
  {
    path: 'driver',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/driver/driver.component').then(m => m.DriverComponent)
  },
  { path: '**', redirectTo: 'login' }
];
