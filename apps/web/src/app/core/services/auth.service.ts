import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface LoginResponse {
  token: string;
  role: string;
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

  getRole(): string | null {
    return localStorage.getItem('role');
  }

  getFullName(): string | null {
    return localStorage.getItem('fullName');
  }

  redirectByRole(): void {
    const role = this.getRole();
    const routeMap: Record<string, string> = {
      Cashier: '/cashier',
      Coordinator: '/coordinator',
      BranchManager: '/branch-manager',
      RestaurantManager: '/restaurant-manager',
      Owner: '/restaurant-manager',
      Driver: '/driver'
    };
    this.router.navigate([routeMap[role ?? ''] ?? '/login']);
  }

  private storeSession(res: LoginResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('role', res.role);
    localStorage.setItem('fullName', res.fullName);
    if (res.branchId) localStorage.setItem('branchId', res.branchId);
    if (res.restaurantId) localStorage.setItem('restaurantId', res.restaurantId);
  }
}
