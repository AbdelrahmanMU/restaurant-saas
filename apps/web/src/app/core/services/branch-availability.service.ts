import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export interface BranchVariantDto {
  id: string;
  branchId: string;
  productVariantId: string;
  productName: string;
  variantName: string;
  menuSectionName: string | null;
  basePrice: number;
  isAvailable: boolean;
  priceOverride: number | null;
}

export interface UpsertBranchVariantRequest {
  isAvailable: boolean;
  priceOverride: number | null;
}

@Injectable({ providedIn: 'root' })
export class BranchAvailabilityService {
  constructor(private api: ApiClientService) {}

  getAvailability(branchId?: string): Observable<BranchVariantDto[]> {
    const params = branchId ? `?branchId=${branchId}` : '';
    return this.api.get<BranchVariantDto[]>(`/branch-availability${params}`);
  }

  upsert(variantId: string, request: UpsertBranchVariantRequest, branchId?: string): Observable<BranchVariantDto> {
    const params = branchId ? `?branchId=${branchId}` : '';
    return this.api.put<BranchVariantDto>(`/branch-availability/${variantId}${params}`, request);
  }
}
