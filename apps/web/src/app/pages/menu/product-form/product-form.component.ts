import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import {
  ProductService,
  ProductDetailDto,
  VariantDto,
  LinkedModifierGroupDto,
  BundleDto,
  BundleSlotDto,
  CreateVariantRequest,
  UpdateVariantRequest,
  CreateBundleSlotRequest,
  LinkModifierGroupRequest,
  AddBundleSlotChoiceRequest,
  ProductVariantSummaryDto
} from '../../../core/services/product.service';
import { MenuSectionService, MenuSectionDto } from '../../../core/services/menu-section.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';
import { ModifierGroupService, ModifierGroupDto } from '../../../core/services/modifier-group.service';
import { BranchAvailabilityService, BranchVariantDto } from '../../../core/services/branch-availability.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnInit {

  // ── Route / Load state ─────────────────────────────────────────────────────
  productId: string | null = null;
  product: ProductDetailDto | null = null;
  loading = true;
  error = '';
  saving = false;
  saveError = '';
  saveSuccess = false;

  activeTab: 'general' | 'variants' | 'modifiers' | 'bundle' | 'availability' = 'general';

  // ── General info form ──────────────────────────────────────────────────────
  formName = '';
  formDescription = '';
  formImageUrl = '';
  formType = 'Simple';
  formMenuSectionId = '';
  formSortOrder = 0;
  formIsActive = true;
  formBranchId = '';

  // ── Lookups ────────────────────────────────────────────────────────────────
  sections: MenuSectionDto[] = [];
  branches: BranchSummary[] = [];
  allModifierGroups: ModifierGroupDto[] = [];
  availabilityItems: BranchVariantDto[] = [];
  availabilityBranchId = '';

  // ── Variant panel ──────────────────────────────────────────────────────────
  variantPanelMode: 'create' | 'edit' | null = null;
  selectedVariant: VariantDto | null = null;
  vFormName = '';
  vFormSku = '';
  vFormPrice = 0;
  vFormIsDefault = false;
  vFormSortOrder = 0;
  vFormIsActive = true;
  vFormLoading = false;
  vFormError = '';

  // ── Modifier linking ───────────────────────────────────────────────────────
  modifierVariantId: string | null = null;
  linkingGroupId = '';
  linkLoading = false;
  linkError = '';

  // ── Bundle slot panel ──────────────────────────────────────────────────────
  slotPanelMode: 'create' | 'edit' | null = null;
  selectedSlot: BundleSlotDto | null = null;
  sFormName = '';
  sFormIsRequired = true;
  sFormMinChoices = 1;
  sFormMaxChoices = 1;
  sFormSortOrder = 0;
  sFormLoading = false;
  sFormError = '';

  // ── Bundle all-variants list (for choice picker) ───────────────────────────
  allVariants: ProductVariantSummaryDto[] = [];
  allVariantsLoading = false;

  // ── Bundle choice adding ───────────────────────────────────────────────────
  addChoiceSlotId: string | null = null;
  addChoiceVariantId = '';
  addChoicePriceDelta = 0;
  addChoiceLoading = false;
  addChoiceError = '';

  // ── Bundle choice editing (price delta) ────────────────────────────────────
  editChoiceId: string | null = null;
  editChoiceSlotId: string | null = null;
  editChoicePriceDelta = 0;
  editChoiceLoading = false;
  editChoiceError = '';

  // ── Availability editing ───────────────────────────────────────────────────
  editingVariantId: string | null = null;
  editIsAvailable = true;
  editPriceOverride: number | null = null;
  editAvailLoading = false;
  editAvailError = '';

  readonly isOwner = this.auth.getActiveRole() === 'Owner';
  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');

  readonly productTypes = [
    { value: 'Simple',       label: 'بسيط' },
    { value: 'VariantBased', label: 'بأحجام/أحجام' },
    { value: 'Customizable', label: 'قابل للتخصيص' },
    { value: 'Bundle',       label: 'وجبة / Bundle' }
  ];

  constructor(
    public auth: AuthService,
    public router: Router,
    private productService: ProductService,
    private menuSectionService: MenuSectionService,
    private branchService: BranchService,
    private modifierGroupService: ModifierGroupService,
    private availabilityService: BranchAvailabilityService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.menuSectionService.getSections().subscribe(s => this.sections = s);
    this.modifierGroupService.getGroups().subscribe(g => this.allModifierGroups = g);
    if (this.isOwnerOrManager) {
      this.branchService.getBranches().subscribe(b => { this.branches = b.filter(x => x.isActive); });
    }

    this.productId = this.route.snapshot.paramMap.get('id');
    if (this.productId) {
      this.loadProduct();
    } else {
      this.loading = false;
    }
  }

  // ── Product loading ────────────────────────────────────────────────────────

  loadProduct(): void {
    this.loading = true;
    this.productService.getProduct(this.productId!).subscribe({
      next: (p) => {
        this.product = p;
        this.populateForm(p);
        this.loading = false;
        if (p.type === 'Bundle') this.ensureBundle();
        this.loadAvailability();
      },
      error: () => { this.error = 'تعذّر تحميل المنتج'; this.loading = false; }
    });
  }

  private populateForm(p: ProductDetailDto): void {
    this.formName         = p.name;
    this.formDescription  = p.description ?? '';
    this.formImageUrl     = p.imageUrl ?? '';
    this.formType         = p.type;
    this.formMenuSectionId = p.menuSectionId ?? '';
    this.formSortOrder    = p.sortOrder;
    this.formIsActive     = p.isActive;
  }

  // ── General tab save ───────────────────────────────────────────────────────

  saveGeneral(): void {
    this.saving = true;
    this.saveError = '';
    this.saveSuccess = false;

    if (this.productId) {
      this.productService.updateProduct(this.productId, {
        menuSectionId: this.formMenuSectionId || null,
        name:          this.formName.trim(),
        description:   this.formDescription.trim() || null,
        imageUrl:      this.formImageUrl.trim() || null,
        sortOrder:     this.formSortOrder,
        isActive:      this.formIsActive
      }).subscribe({
        next: (p) => {
          this.product = p;
          this.saving = false;
          this.saveSuccess = true;
          setTimeout(() => this.saveSuccess = false, 2000);
        },
        error: (err) => { this.saveError = err?.error?.message ?? 'حدث خطأ'; this.saving = false; }
      });
    } else {
      this.productService.createProduct({
        branchId:      this.isOwnerOrManager ? (this.formBranchId || null) : null,
        menuSectionId: this.formMenuSectionId || null,
        name:          this.formName.trim(),
        description:   this.formDescription.trim() || null,
        imageUrl:      this.formImageUrl.trim() || null,
        type:          this.formType,
        sortOrder:     this.formSortOrder
      }).subscribe({
        next: (p) => { this.router.navigate(['/menu/products', p.id], { replaceUrl: true }); },
        error: (err) => { this.saveError = err?.error?.message ?? 'حدث خطأ'; this.saving = false; }
      });
    }
  }

  // ── Variant methods ────────────────────────────────────────────────────────

  openCreateVariant(): void {
    this.selectedVariant = null;
    this.vFormName = '';
    this.vFormSku = '';
    this.vFormPrice = 0;
    this.vFormIsDefault = false;
    this.vFormSortOrder = this.product?.variants?.length ?? 0;
    this.vFormIsActive = true;
    this.vFormError = '';
    this.variantPanelMode = 'create';
  }

  openEditVariant(v: VariantDto): void {
    this.selectedVariant = v;
    this.vFormName      = v.name;
    this.vFormSku       = v.sku ?? '';
    this.vFormPrice     = v.price;
    this.vFormIsDefault = v.isDefault;
    this.vFormSortOrder = v.sortOrder;
    this.vFormIsActive  = v.isActive;
    this.vFormError     = '';
    this.variantPanelMode = 'edit';
  }

  closeVariantPanel(): void {
    this.variantPanelMode = null;
    this.selectedVariant = null;
    this.vFormError = '';
  }

  submitVariant(): void {
    if (!this.vFormName.trim()) { this.vFormError = 'اسم المتغيّر مطلوب'; return; }
    this.vFormLoading = true;
    this.vFormError = '';

    if (this.variantPanelMode === 'create') {
      const req: CreateVariantRequest = {
        name:      this.vFormName.trim(),
        sku:       this.vFormSku.trim() || null,
        price:     this.vFormPrice,
        isDefault: this.vFormIsDefault,
        sortOrder: this.vFormSortOrder
      };
      this.productService.createVariant(this.productId!, req).subscribe({
        next: (v) => {
          this.product!.variants = [...(this.product!.variants ?? []), v];
          this.vFormLoading = false;
          this.closeVariantPanel();
        },
        error: (err) => { this.vFormError = err?.error?.message ?? 'حدث خطأ'; this.vFormLoading = false; }
      });
    } else {
      const req: UpdateVariantRequest = {
        name:      this.vFormName.trim(),
        sku:       this.vFormSku.trim() || null,
        price:     this.vFormPrice,
        isDefault: this.vFormIsDefault,
        sortOrder: this.vFormSortOrder,
        isActive:  this.vFormIsActive
      };
      this.productService.updateVariant(this.productId!, this.selectedVariant!.id, req).subscribe({
        next: (updated) => {
          this.product!.variants = this.product!.variants.map(v =>
            v.id === updated.id ? { ...updated, modifierGroups: v.modifierGroups } : v
          );
          this.vFormLoading = false;
          this.closeVariantPanel();
        },
        error: (err) => { this.vFormError = err?.error?.message ?? 'حدث خطأ'; this.vFormLoading = false; }
      });
    }
  }

  deactivateVariant(v: VariantDto): void {
    this.productService.deactivateVariant(this.productId!, v.id).subscribe({
      next: () => {
        this.product!.variants = this.product!.variants.map(x =>
          x.id === v.id ? { ...x, isActive: false } : x
        );
      },
      error: (err) => {
        const msg: string = err?.error?.message ?? '';
        this.vFormError = msg || 'تعذّر إيقاف المتغير';
      }
    });
  }

  // ── Modifier linking ───────────────────────────────────────────────────────

  showModifiersFor(variantId: string): void {
    this.modifierVariantId = variantId;
    this.linkingGroupId = '';
    this.linkError = '';
  }

  get currentVariant(): VariantDto | null {
    return this.product?.variants.find(v => v.id === this.modifierVariantId) ?? null;
  }

  get currentVariantModifiers(): LinkedModifierGroupDto[] {
    return this.currentVariant?.modifierGroups ?? [];
  }

  get availableGroupsToLink(): ModifierGroupDto[] {
    const linkedIds = new Set(this.currentVariantModifiers.map(mg => mg.modifierGroupId));
    return this.allModifierGroups.filter(g => g.isActive && !linkedIds.has(g.id));
  }

  linkGroup(): void {
    if (!this.linkingGroupId) return;
    this.linkLoading = true;
    this.linkError = '';

    const req: LinkModifierGroupRequest = {
      modifierGroupId: this.linkingGroupId,
      sortOrder: this.currentVariantModifiers.length
    };

    this.productService.linkModifierGroup(this.productId!, this.modifierVariantId!, req).subscribe({
      next: (linked) => {
        const variant = this.product!.variants.find(v => v.id === this.modifierVariantId);
        if (variant) {
          variant.modifierGroups = [...variant.modifierGroups, linked];
        }
        this.linkingGroupId = '';
        this.linkLoading = false;
      },
      error: (err) => { this.linkError = err?.error?.message ?? 'حدث خطأ'; this.linkLoading = false; }
    });
  }

  unlinkGroup(variant: VariantDto, linkId: string, groupId: string): void {
    this.productService.unlinkModifierGroup(this.productId!, variant.id, linkId).subscribe({
      next: () => {
        const v = this.product!.variants.find(x => x.id === variant.id);
        if (v) {
          v.modifierGroups = v.modifierGroups.filter(mg => mg.linkId !== linkId);
        }
      },
      error: (err) => { this.linkError = err?.error?.message ?? 'حدث خطأ عند الإزالة'; }
    });
  }

  // ── Bundle methods ─────────────────────────────────────────────────────────

  loadAllVariants(): void {
    if (this.allVariants.length) return; // already loaded
    this.allVariantsLoading = true;
    this.productService.getAllVariants().subscribe({
      next: (v) => { this.allVariants = v; this.allVariantsLoading = false; },
      error: () => { this.allVariantsLoading = false; }
    });
  }

  availableVariantsForSlot(slotId: string): ProductVariantSummaryDto[] {
    const slot = this.product?.bundle?.slots.find(s => s.id === slotId);
    const linkedIds = new Set(slot?.choices.map(c => c.productVariantId) ?? []);
    return this.allVariants.filter(v => !linkedIds.has(v.variantId));
  }

  startEditChoice(slotId: string, choiceId: string, currentDelta: number): void {
    this.editChoiceSlotId = slotId;
    this.editChoiceId = choiceId;
    this.editChoicePriceDelta = currentDelta;
    this.editChoiceError = '';
  }

  cancelEditChoice(): void {
    this.editChoiceId = null;
    this.editChoiceSlotId = null;
    this.editChoiceError = '';
  }

  saveEditChoice(): void {
    if (!this.editChoiceId || !this.editChoiceSlotId) return;
    this.editChoiceLoading = true;
    this.editChoiceError = '';

    this.productService.updateSlotChoice(this.productId!, this.editChoiceSlotId, this.editChoiceId, {
      priceDelta: this.editChoicePriceDelta
    }).subscribe({
      next: (updated) => {
        const slot = this.product!.bundle!.slots.find(s => s.id === this.editChoiceSlotId);
        if (slot) {
          slot.choices = slot.choices.map(c => c.id === updated.id ? updated : c);
        }
        this.editChoiceLoading = false;
        this.cancelEditChoice();
      },
      error: (err) => { this.editChoiceError = err?.error?.message ?? 'حدث خطأ'; this.editChoiceLoading = false; }
    });
  }

  ensureBundle(): void {
    if (!this.product?.bundle) {
      this.productService.ensureBundle(this.productId!).subscribe({
        next: (b) => { this.product!.bundle = b; }
      });
    }
  }

  openCreateSlot(): void {
    this.selectedSlot = null;
    this.sFormName = '';
    this.sFormIsRequired = true;
    this.sFormMinChoices = 1;
    this.sFormMaxChoices = 1;
    this.sFormSortOrder = this.product?.bundle?.slots?.length ?? 0;
    this.sFormError = '';
    this.slotPanelMode = 'create';
  }

  openEditSlot(slot: BundleSlotDto): void {
    this.selectedSlot    = slot;
    this.sFormName       = slot.name;
    this.sFormIsRequired = slot.isRequired;
    this.sFormMinChoices = slot.minChoices;
    this.sFormMaxChoices = slot.maxChoices;
    this.sFormSortOrder  = slot.sortOrder;
    this.sFormError      = '';
    this.slotPanelMode = 'edit';
  }

  closeSlotPanel(): void {
    this.slotPanelMode = null;
    this.selectedSlot = null;
    this.sFormError = '';
  }

  submitSlot(): void {
    if (!this.sFormName.trim()) { this.sFormError = 'اسم الفتحة مطلوب'; return; }
    this.sFormLoading = true;
    this.sFormError = '';

    const req: CreateBundleSlotRequest = {
      name:       this.sFormName.trim(),
      isRequired: this.sFormIsRequired,
      minChoices: this.sFormMinChoices,
      maxChoices: this.sFormMaxChoices,
      sortOrder:  this.sFormSortOrder
    };

    if (this.slotPanelMode === 'create') {
      this.productService.addBundleSlot(this.productId!, req).subscribe({
        next: (slot) => {
          this.product!.bundle!.slots = [...(this.product!.bundle!.slots ?? []), slot];
          this.sFormLoading = false;
          this.closeSlotPanel();
        },
        error: (err) => { this.sFormError = err?.error?.message ?? 'حدث خطأ'; this.sFormLoading = false; }
      });
    } else {
      this.productService.updateBundleSlot(this.productId!, this.selectedSlot!.id, req).subscribe({
        next: (updated) => {
          this.product!.bundle!.slots = this.product!.bundle!.slots.map(s =>
            s.id === updated.id ? { ...updated, choices: s.choices } : s
          );
          this.sFormLoading = false;
          this.closeSlotPanel();
        },
        error: (err) => { this.sFormError = err?.error?.message ?? 'حدث خطأ'; this.sFormLoading = false; }
      });
    }
  }

  deleteSlot(slot: BundleSlotDto): void {
    this.sFormError = '';
    this.productService.deleteBundleSlot(this.productId!, slot.id).subscribe({
      next: () => {
        this.product!.bundle!.slots = this.product!.bundle!.slots.filter(s => s.id !== slot.id);
      },
      error: (err) => {
        const msg: string = err?.error?.message ?? '';
        this.sFormError = msg || 'تعذّر حذف الخانة';
      }
    });
  }

  showChoicesFor(slotId: string): void {
    this.addChoiceSlotId = slotId;
    this.addChoiceVariantId = '';
    this.addChoicePriceDelta = 0;
    this.addChoiceError = '';
    this.loadAllVariants();
  }

  submitChoice(): void {
    if (!this.addChoiceVariantId) return;
    this.addChoiceLoading = true;
    this.addChoiceError = '';

    const req: AddBundleSlotChoiceRequest = {
      productVariantId: this.addChoiceVariantId,
      priceDelta: this.addChoicePriceDelta
    };

    this.productService.addSlotChoice(this.productId!, this.addChoiceSlotId!, req).subscribe({
      next: (choice) => {
        const slot = this.product!.bundle!.slots.find(s => s.id === this.addChoiceSlotId);
        if (slot) {
          slot.choices = [...slot.choices, choice];
        }
        this.addChoiceVariantId = '';
        this.addChoicePriceDelta = 0;
        this.addChoiceSlotId = null;
        this.addChoiceLoading = false;
      },
      error: (err) => { this.addChoiceError = err?.error?.message ?? 'حدث خطأ'; this.addChoiceLoading = false; }
    });
  }

  removeChoice(slotId: string, choiceId: string): void {
    this.productService.removeSlotChoice(this.productId!, slotId, choiceId).subscribe({
      next: () => {
        const slot = this.product!.bundle!.slots.find(s => s.id === slotId);
        if (slot) {
          slot.choices = slot.choices.filter(c => c.id !== choiceId);
        }
      }
    });
  }

  get currentSlot(): BundleSlotDto | null {
    return this.product?.bundle?.slots.find(s => s.id === this.addChoiceSlotId) ?? null;
  }

  // ── Availability methods ───────────────────────────────────────────────────

  loadAvailability(): void {
    if (this.isOwnerOrManager && !this.availabilityBranchId) return;

    const branchId = this.isOwnerOrManager ? this.availabilityBranchId : undefined;
    this.availabilityService.getAvailability(branchId).subscribe({
      next: (items) => {
        const variantIds = new Set(this.product?.variants.map(v => v.id) ?? []);
        this.availabilityItems = items.filter(i => variantIds.has(i.productVariantId));
      }
    });
  }

  startEditAvail(item: BranchVariantDto): void {
    this.editingVariantId  = item.productVariantId;
    this.editIsAvailable   = item.isAvailable;
    this.editPriceOverride = item.priceOverride;
    this.editAvailError    = '';
  }

  cancelEditAvail(): void {
    this.editingVariantId = null;
    this.editAvailError   = '';
  }

  saveAvail(item: BranchVariantDto): void {
    this.editAvailLoading = true;
    this.editAvailError   = '';

    const branchId = this.isOwnerOrManager ? this.availabilityBranchId : undefined;
    this.availabilityService.upsert(item.productVariantId, {
      isAvailable:   this.editIsAvailable,
      priceOverride: this.editPriceOverride
    }, branchId).subscribe({
      next: (updated) => {
        this.availabilityItems = this.availabilityItems.map(x =>
          x.productVariantId === updated.productVariantId ? updated : x
        );
        this.editingVariantId = null;
        this.editAvailLoading = false;
      },
      error: (err) => { this.editAvailError = err?.error?.message ?? 'حدث خطأ'; this.editAvailLoading = false; }
    });
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  get isBundle(): boolean {
    return this.formType === 'Bundle';
  }

  get isEditMode(): boolean {
    return this.productId !== null;
  }

  typeLabel(t: string): string {
    switch (t) {
      case 'Simple':       return 'بسيط';
      case 'VariantBased': return 'بأحجام';
      case 'Customizable': return 'قابل للتخصيص';
      case 'Bundle':       return 'وجبة';
      default:             return t;
    }
  }
}
