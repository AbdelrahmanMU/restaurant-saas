import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface CreateInviteRequest {
  phoneNumber: string;
  role: string;
  branchId?: string | null;
}

export interface CreateInviteResponse {
  inviteId: string;
  activationLink: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class InviteService {
  constructor(private api: ApiClientService) {}

  createInvite(data: CreateInviteRequest): Observable<CreateInviteResponse> {
    return this.api.post<CreateInviteResponse>('/invites', data);
  }
}
