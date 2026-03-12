import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface UserSummary {
  id: string;
  fullName: string;
  phoneNumber: string;
  roles: string[];
  branchId: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface UserRoleEntry {
  id: string;
  role: string;
  branchId: string | null;
}

export interface UserDetail {
  id: string;
  fullName: string;
  phoneNumber: string;
  roleEntries: UserRoleEntry[];
  branchId: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface AddRoleRequest {
  role: string;
  branchId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserManagementService {
  constructor(private api: ApiClientService) {}

  getUsers(): Observable<UserSummary[]> {
    return this.api.get<UserSummary[]>('/users');
  }

  getUser(id: string): Observable<UserDetail> {
    return this.api.get<UserDetail>(`/users/${id}`);
  }

  addRole(userId: string, request: AddRoleRequest): Observable<UserRoleEntry> {
    return this.api.post<UserRoleEntry>(`/users/${userId}/roles`, request);
  }

  removeRole(userId: string, roleId: string): Observable<void> {
    return this.api.delete<void>(`/users/${userId}/roles/${roleId}`);
  }
}
