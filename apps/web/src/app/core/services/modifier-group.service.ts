import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface ModifierOptionDto {
  id: string;
  name: string;
  priceDelta: number;
  isDefault: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface ModifierGroupDto {
  id: string;
  branchId: string;
  branchName: string;
  name: string;
  selectionType: 'Single' | 'Multiple';
  isRequired: boolean;
  minSelections: number;
  maxSelections: number | null;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  options: ModifierOptionDto[];
}

export interface CreateModifierGroupRequest {
  branchId: string | null;
  name: string;
  selectionType: string;
  isRequired: boolean;
  minSelections: number;
  maxSelections: number | null;
  sortOrder: number;
}

export interface UpdateModifierGroupRequest {
  name: string;
  selectionType: string;
  isRequired: boolean;
  minSelections: number;
  maxSelections: number | null;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateModifierOptionRequest {
  name: string;
  priceDelta: number;
  isDefault: boolean;
  sortOrder: number;
}

export interface UpdateModifierOptionRequest {
  name: string;
  priceDelta: number;
  isDefault: boolean;
  sortOrder: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class ModifierGroupService {
  constructor(private api: ApiClientService) {}

  getGroups(): Observable<ModifierGroupDto[]> {
    return this.api.get<ModifierGroupDto[]>('/modifier-groups');
  }

  getGroup(id: string): Observable<ModifierGroupDto> {
    return this.api.get<ModifierGroupDto>(`/modifier-groups/${id}`);
  }

  createGroup(request: CreateModifierGroupRequest): Observable<ModifierGroupDto> {
    return this.api.post<ModifierGroupDto>('/modifier-groups', request);
  }

  updateGroup(id: string, request: UpdateModifierGroupRequest): Observable<ModifierGroupDto> {
    return this.api.put<ModifierGroupDto>(`/modifier-groups/${id}`, request);
  }

  deactivateGroup(id: string): Observable<void> {
    return this.api.delete<void>(`/modifier-groups/${id}`);
  }

  addOption(groupId: string, request: CreateModifierOptionRequest): Observable<ModifierOptionDto> {
    return this.api.post<ModifierOptionDto>(`/modifier-groups/${groupId}/options`, request);
  }

  updateOption(groupId: string, optionId: string, request: UpdateModifierOptionRequest): Observable<ModifierOptionDto> {
    return this.api.put<ModifierOptionDto>(`/modifier-groups/${groupId}/options/${optionId}`, request);
  }

  deactivateOption(groupId: string, optionId: string): Observable<void> {
    return this.api.delete<void>(`/modifier-groups/${groupId}/options/${optionId}`);
  }
}
