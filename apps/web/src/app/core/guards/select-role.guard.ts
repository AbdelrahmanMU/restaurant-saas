import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const selectRoleGuard: CanActivateFn = () => {
  const router = inject(Router);
  const auth = inject(AuthService);

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const roles = auth.getRoles();

  if (roles.length === 0) {
    // Broken session — clear and restart
    auth.logout();
    return false;
  }

  if (roles.length === 1) {
    // No need to pick — silently redirect to their single dashboard
    auth.setActiveRole(roles[0]);
    auth.navigateForRole(roles[0]);
    return false;
  }

  // Multi-role: show the picker
  return true;
};
