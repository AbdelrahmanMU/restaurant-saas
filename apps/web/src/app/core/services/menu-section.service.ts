import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface MenuSectionDto {
  id: string;
  branchId: string;
  branchName: string;
  name: string;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateMenuSectionRequest {
  branchId: string | null;
  name: string;
  description: string | null;
  sortOrder: number;
}

export interface UpdateMenuSectionRequest {
  name: string;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class MenuSectionService {
  constructor(private api: ApiClientService) {}

  getSections(): Observable<MenuSectionDto[]> {
    return this.api.get<MenuSectionDto[]>('/menu-sections');
  }

  getSection(id: string): Observable<MenuSectionDto> {
    return this.api.get<MenuSectionDto>(`/menu-sections/${id}`);
  }

  createSection(request: CreateMenuSectionRequest): Observable<MenuSectionDto> {
    return this.api.post<MenuSectionDto>('/menu-sections', request);
  }

  updateSection(id: string, request: UpdateMenuSectionRequest): Observable<MenuSectionDto> {
    return this.api.put<MenuSectionDto>(`/menu-sections/${id}`, request);
  }

  deactivateSection(id: string): Observable<void> {
    return this.api.delete<void>(`/menu-sections/${id}`);
  }
}
