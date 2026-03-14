import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface CategoryDto {
  id: string;
  branchId: string;
  branchName: string;
  name: string;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCategoryRequest {
  branchId: string | null;  // required for Owner; ignored for BranchManager
  name: string;
  description: string | null;
  sortOrder: number;
}

export interface UpdateCategoryRequest {
  name: string;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  constructor(private api: ApiClientService) {}

  getCategories(): Observable<CategoryDto[]> {
    return this.api.get<CategoryDto[]>('/categories');
  }

  getCategory(id: string): Observable<CategoryDto> {
    return this.api.get<CategoryDto>(`/categories/${id}`);
  }

  createCategory(request: CreateCategoryRequest): Observable<CategoryDto> {
    return this.api.post<CategoryDto>('/categories', request);
  }

  updateCategory(id: string, request: UpdateCategoryRequest): Observable<CategoryDto> {
    return this.api.put<CategoryDto>(`/categories/${id}`, request);
  }

  /** Soft-delete: deactivates the category (sets IsActive = false) */
  deactivateCategory(id: string): Observable<void> {
    return this.api.delete<void>(`/categories/${id}`);
  }
}
