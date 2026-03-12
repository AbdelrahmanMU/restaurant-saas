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
  Cashier:           '/cashier',
  Coordinator:       '/coordinator',
  BranchManager:     '/branch-manager',
  RestaurantManager: '/restaurant-manager',
  Owner:             '/restaurant-manager',
  Driver:            '/driver'
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private api: ApiClientService, private router: Router) {}

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

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // Returns all roles assigned to this user
  getRoles(): string[] {
    const raw = localStorage.getItem('roles');
    if (!raw) return [];
    try { return JSON.parse(raw) as string[]; } catch { return []; }
  }

  // The role the user selected (or the only role if they have one)
  getActiveRole(): string | null {
    return localStorage.getItem('activeRole');
  }

  // Kept for backward compat with components that call getRole()
  getRole(): string | null {
    return this.getActiveRole() ?? this.getRoles()[0] ?? null;
  }

  getFullName(): string | null {
    return localStorage.getItem('fullName');
  }

  hasRole(role: string): boolean {
    return this.getRoles().includes(role);
  }

  setActiveRole(role: string): void {
    localStorage.setItem('activeRole', role);
  }

  // After login / register / activate:
  //   1 role  → set activeRole immediately, redirect to dashboard
  //   >1 role → go to /select-role so user picks
  redirectByRole(): void {
    const roles = this.getRoles();
    if (roles.length === 1) {
      this.setActiveRole(roles[0]);
      this.navigateForRole(roles[0]);
    } else if (roles.length > 1) {
      this.router.navigate(['/select-role']);
    } else {
      this.router.navigate(['/login']);
    }
  }

  navigateForRole(role: string): void {
    this.router.navigate([ROUTE_MAP[role] ?? '/login']);
  }

  private storeSession(res: LoginResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('roles', JSON.stringify(res.roles));
    localStorage.setItem('fullName', res.fullName);
    if (res.branchId) localStorage.setItem('branchId', res.branchId);
    if (res.restaurantId) localStorage.setItem('restaurantId', res.restaurantId);

    // Auto-set activeRole only when there's exactly one role
    if (res.roles.length === 1) {
      localStorage.setItem('activeRole', res.roles[0]);
    }
  }
}
