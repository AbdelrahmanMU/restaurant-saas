import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';

export type ProductType = 'Simple' | 'VariantBased' | 'Customizable' | 'Bundle';

export interface ModifierOptionDto {
  id: string;
  name: string;
  priceDelta: number;
  isDefault: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface LinkedModifierGroupDto {
  linkId: string;
  modifierGroupId: string;
  name: string;
  selectionType: string;
  isRequired: boolean;
  minSelections: number;
  maxSelections: number | null;
  sortOrder: number;
  options: ModifierOptionDto[];
}

export interface VariantDto {
  id: string;
  name: string;
  sku: string | null;
  price: number;
  isDefault: boolean;
  isActive: boolean;
  sortOrder: number;
  modifierGroups: LinkedModifierGroupDto[];
}

export interface BundleSlotChoiceDto {
  id: string;
  productVariantId: string;
  productName: string;
  variantName: string;
  basePrice: number;
  priceDelta: number;
}

export interface BundleSlotDto {
  id: string;
  name: string;
  isRequired: boolean;
  minChoices: number;
  maxChoices: number;
  sortOrder: number;
  choices: BundleSlotChoiceDto[];
}

export interface BundleDto {
  id: string;
  productId: string;
  slots: BundleSlotDto[];
}

export interface ProductSummaryDto {
  id: string;
  branchId: string;
  branchName: string;
  menuSectionId: string | null;
  menuSectionName: string | null;
  name: string;
  description: string | null;
  imageUrl: string | null;
  type: ProductType;
  sortOrder: number;
  isActive: boolean;
  basePrice: number | null;
  variantCount: number;
  createdAt: string;
}

export interface ProductDetailDto extends ProductSummaryDto {
  variants: VariantDto[];
  bundle: BundleDto | null;
}

export interface CreateProductRequest {
  branchId: string | null;
  menuSectionId: string | null;
  name: string;
  description: string | null;
  imageUrl: string | null;
  type: string;
  sortOrder: number;
}

export interface UpdateProductRequest {
  menuSectionId: string | null;
  name: string;
  description: string | null;
  imageUrl: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateVariantRequest {
  name: string;
  sku: string | null;
  price: number;
  isDefault: boolean;
  sortOrder: number;
}

export interface UpdateVariantRequest {
  name: string;
  sku: string | null;
  price: number;
  isDefault: boolean;
  sortOrder: number;
  isActive: boolean;
}

export interface LinkModifierGroupRequest {
  modifierGroupId: string;
  sortOrder: number;
}

export interface CreateBundleSlotRequest {
  name: string;
  isRequired: boolean;
  minChoices: number;
  maxChoices: number;
  sortOrder: number;
}

export interface UpdateBundleSlotRequest {
  name: string;
  isRequired: boolean;
  minChoices: number;
  maxChoices: number;
  sortOrder: number;
}

export interface AddBundleSlotChoiceRequest {
  productVariantId: string;
  priceDelta: number;
}

export interface UpdateBundleSlotChoiceRequest {
  priceDelta: number;
}

export interface ProductVariantSummaryDto {
  variantId: string;
  productId: string;
  productName: string;
  variantName: string;
  price: number;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  constructor(private api: ApiClientService) {}

  // Products
  getProducts(): Observable<ProductSummaryDto[]> {
    return this.api.get<ProductSummaryDto[]>('/products');
  }

  getProduct(id: string): Observable<ProductDetailDto> {
    return this.api.get<ProductDetailDto>(`/products/${id}`);
  }

  createProduct(request: CreateProductRequest): Observable<ProductDetailDto> {
    return this.api.post<ProductDetailDto>('/products', request);
  }

  updateProduct(id: string, request: UpdateProductRequest): Observable<ProductDetailDto> {
    return this.api.put<ProductDetailDto>(`/products/${id}`, request);
  }

  deactivateProduct(id: string): Observable<void> {
    return this.api.delete<void>(`/products/${id}`);
  }

  // All variants flat list (for bundle choice picker)
  getAllVariants(): Observable<ProductVariantSummaryDto[]> {
    return this.api.get<ProductVariantSummaryDto[]>('/products/variants');
  }

  // Variants
  getVariants(productId: string): Observable<VariantDto[]> {
    return this.api.get<VariantDto[]>(`/products/${productId}/variants`);
  }

  createVariant(productId: string, request: CreateVariantRequest): Observable<VariantDto> {
    return this.api.post<VariantDto>(`/products/${productId}/variants`, request);
  }

  updateVariant(productId: string, variantId: string, request: UpdateVariantRequest): Observable<VariantDto> {
    return this.api.put<VariantDto>(`/products/${productId}/variants/${variantId}`, request);
  }

  deactivateVariant(productId: string, variantId: string): Observable<void> {
    return this.api.delete<void>(`/products/${productId}/variants/${variantId}`);
  }

  // Modifier Group Links
  getVariantModifierGroups(productId: string, variantId: string): Observable<LinkedModifierGroupDto[]> {
    return this.api.get<LinkedModifierGroupDto[]>(`/products/${productId}/variants/${variantId}/modifier-groups`);
  }

  linkModifierGroup(productId: string, variantId: string, request: LinkModifierGroupRequest): Observable<LinkedModifierGroupDto> {
    return this.api.post<LinkedModifierGroupDto>(`/products/${productId}/variants/${variantId}/modifier-groups`, request);
  }

  unlinkModifierGroup(productId: string, variantId: string, groupId: string): Observable<void> {
    return this.api.delete<void>(`/products/${productId}/variants/${variantId}/modifier-groups/${groupId}`);
  }

  // Bundle
  getBundle(productId: string): Observable<BundleDto> {
    return this.api.get<BundleDto>(`/products/${productId}/bundle`);
  }

  ensureBundle(productId: string): Observable<BundleDto> {
    return this.api.post<BundleDto>(`/products/${productId}/bundle/ensure`, {});
  }

  addBundleSlot(productId: string, request: CreateBundleSlotRequest): Observable<BundleSlotDto> {
    return this.api.post<BundleSlotDto>(`/products/${productId}/bundle/slots`, request);
  }

  updateBundleSlot(productId: string, slotId: string, request: UpdateBundleSlotRequest): Observable<BundleSlotDto> {
    return this.api.put<BundleSlotDto>(`/products/${productId}/bundle/slots/${slotId}`, request);
  }

  deleteBundleSlot(productId: string, slotId: string): Observable<void> {
    return this.api.delete<void>(`/products/${productId}/bundle/slots/${slotId}`);
  }

  addSlotChoice(productId: string, slotId: string, request: AddBundleSlotChoiceRequest): Observable<BundleSlotChoiceDto> {
    return this.api.post<BundleSlotChoiceDto>(`/products/${productId}/bundle/slots/${slotId}/choices`, request);
  }

  updateSlotChoice(productId: string, slotId: string, choiceId: string, request: UpdateBundleSlotChoiceRequest): Observable<BundleSlotChoiceDto> {
    return this.api.put<BundleSlotChoiceDto>(`/products/${productId}/bundle/slots/${slotId}/choices/${choiceId}`, request);
  }

  removeSlotChoice(productId: string, slotId: string, choiceId: string): Observable<void> {
    return this.api.delete<void>(`/products/${productId}/bundle/slots/${slotId}/choices/${choiceId}`);
  }
}
