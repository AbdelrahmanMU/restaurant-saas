import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { selectRoleGuard } from './core/guards/select-role.guard';

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
    path: 'branch-manager/staff',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/branch-manager/staff/staff.component').then(m => m.StaffComponent)
  },
  {
    path: 'restaurant-manager',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/restaurant-manager/restaurant-manager.component').then(m => m.RestaurantManagerComponent)
  },
  {
    path: 'restaurant-manager/staff',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/branch-manager/staff/staff.component').then(m => m.StaffComponent)
  },
  {
    path: 'owner/users',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/owner/users/owner-users.component').then(m => m.OwnerUsersComponent)
  },
  {
    path: 'owner/branches',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/owner/branches/owner-branches.component').then(m => m.OwnerBranchesComponent)
  },
  {
    path: 'menu',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/menu-hub/menu-hub.component').then(m => m.MenuHubComponent)
  },
  {
    path: 'menu/sections',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/sections/menu-sections.component').then(m => m.MenuSectionsComponent)
  },
  {
    path: 'menu/products',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/products/products.component').then(m => m.ProductsComponent)
  },
  {
    path: 'menu/products/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/product-form/product-form.component').then(m => m.ProductFormComponent)
  },
  { 
    path: 'menu/modifier-groups',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/modifier-groups/modifier-groups.component').then(m => m.ModifierGroupsComponent)
  },
  {
    path: 'menu/branch-availability',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/menu/branch-availability/branch-availability.component').then(m => m.BranchAvailabilityComponent)
  },
  {
    path: 'driver',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/driver/driver.component').then(m => m.DriverComponent)
  },
  {
    path: 'select-role',
    canActivate: [selectRoleGuard],
    loadComponent: () => import('./pages/select-role/select-role.component').then(m => m.SelectRoleComponent)
  },
  { path: '**', redirectTo: 'login' }
];
