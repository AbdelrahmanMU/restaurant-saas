import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface LoginResponse {
  token: string;
  roles: string[];
  fullName: string;
  branchId: string | null;
  restaurantId: string | null;
}

export interface RegisterOwnerRequest {
  fullName: string;
  phoneNumber: string;
  password: string;
  restaurantName: string;
}

export interface ActivateInviteRequest {
  inviteToken: string;
  fullName: string;
  phoneNumber: string;
  password: string;
}

const ROUTE_MAP: Record<string, string> = {
  Owner:             '/restaurant-manager',
  RestaurantManager: '/restaurant-manager',
  BranchManager:     '/branch-manager',
  Cashier:           '/cashier',
  Coordinator:       '/coordinator',
  Driver:            '/driver'
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private api: ApiClientService, private router: Router) {}

  // ─── API calls ────────────────────────────────────────────────────────────

  login(phoneNumber: string, password: string): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/auth/login', { phoneNumber, password }).pipe(
      tap(res => this.storeSession(res))
    );
  }

  registerOwner(data: RegisterOwnerRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/auth/register-owner', data).pipe(
      tap(res => this.storeSession(res))
    );
  }

  activateInvite(data: ActivateInviteRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/auth/activate-invite', data).pipe(
      tap(res => this.storeSession(res))
    );
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  // ─── Session queries ──────────────────────────────────────────────────────

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  /** All roles assigned to this user in the backend. Never use for implicit navigation. */
  getRoles(): string[] {
    const raw = localStorage.getItem('roles');
    if (!raw) return [];
    try { return JSON.parse(raw) as string[]; } catch { return []; }
  }

  /**
   * The role the user explicitly selected for this session.
   * This is the ONLY source of truth for current app context.
   */
  getActiveRole(): string | null {
    return localStorage.getItem('activeRole');
  }

  /**
   * Safe active-role accessor for display use only.
   * Does NOT fall back to roles[] — returns null if nothing is committed.
   */
  getRole(): string | null {
    return this.getActiveRole();
  }

  getFullName(): string | null {
    return localStorage.getItem('fullName');
  }

  hasRole(role: string): boolean {
    return this.getRoles().includes(role);
  }

  // ─── Role commitment ──────────────────────────────────────────────────────

  /**
   * Explicitly commit a role. Only called from SelectRoleComponent or
   * storeSession (when there is exactly one role to auto-commit).
   */
  setActiveRole(role: string): void {
    localStorage.setItem('activeRole', role);
  }

  // ─── Navigation ───────────────────────────────────────────────────────────

  /**
   * Called once after login / register / activate.
   * If activeRole is already committed, go straight to its dashboard.
   * Otherwise decide based on number of available roles.
   */
  redirectByRole(): void {
    const committed = this.getActiveRole();
    if (committed) {
      this.navigateForRole(committed);
      return;
    }

    const roles = this.getRoles();
    if (roles.length === 0) {
      this.router.navigate(['/login']);
    } else if (roles.length === 1) {
      this.setActiveRole(roles[0]);
      this.navigateForRole(roles[0]);
    } else {
      this.router.navigate(['/select-role']);
    }
  }

  /**
   * Navigate to the dashboard of the given role.
   * Should only be called with a known, committed role string.
   */
  navigateForRole(role: string): void {
    this.router.navigate([ROUTE_MAP[role] ?? '/login']);
  }

  /**
   * Navigate to the dashboard of the current committed activeRole.
   * Use this for all "go back to my dashboard" actions inside pages.
   */
  goToDashboard(): void {
    const role = this.getActiveRole();
    if (role) {
      this.navigateForRole(role);
    } else {
      this.router.navigate(['/select-role']);
    }
  }

  // ─── Private ──────────────────────────────────────────────────────────────

  private storeSession(res: LoginResponse): void {
    localStorage.removeItem('activeRole'); // always clear on fresh session
    localStorage.setItem('token', res.token);
    localStorage.setItem('roles', JSON.stringify(res.roles));
    localStorage.setItem('fullName', res.fullName);
    if (res.branchId) localStorage.setItem('branchId', res.branchId);
    if (res.restaurantId) localStorage.setItem('restaurantId', res.restaurantId);

    // Auto-commit only when there is no choice to make
    if (res.roles.length === 1) {
      this.setActiveRole(res.roles[0]);
    }
  }
}
