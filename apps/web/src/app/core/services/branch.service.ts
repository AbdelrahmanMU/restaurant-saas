import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface BranchManager {
  userId: string;
  fullName: string;
  phoneNumber: string;
}

export interface BranchSummary {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
  createdAt: string;
  manager: BranchManager | null;
}

export interface CreateBranchRequest {
  name: string;
  address: string;
}

export interface UpdateBranchRequest {
  name: string;
  address: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class BranchService {
  constructor(private api: ApiClientService) {}

  getBranches(): Observable<BranchSummary[]> {
    return this.api.get<BranchSummary[]>('/branches');
  }

  getBranch(id: string): Observable<BranchSummary> {
    return this.api.get<BranchSummary>(`/branches/${id}`);
  }

  createBranch(request: CreateBranchRequest): Observable<BranchSummary> {
    return this.api.post<BranchSummary>('/branches', request);
  }

  updateBranch(id: string, request: UpdateBranchRequest): Observable<BranchSummary> {
    return this.api.put<BranchSummary>(`/branches/${id}`, request);
  }

  assignManager(branchId: string, userId: string): Observable<BranchSummary> {
    return this.api.post<BranchSummary>(`/branches/${branchId}/manager`, { userId });
  }

  removeManager(branchId: string): Observable<void> {
    return this.api.delete<void>(`/branches/${branchId}/manager`);
  }
}
