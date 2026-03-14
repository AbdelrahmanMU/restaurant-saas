import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ProductService, ProductSummaryDto, CreateProductRequest } from '../../../core/services/product.service';
import { MenuSectionService, MenuSectionDto } from '../../../core/services/menu-section.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  products: ProductSummaryDto[] = [];
  loading = true;
  error = '';

  // ── Create panel ─────────────────────────────────────────────────────────────
  createPanelOpen = false;

  // Form state
  formName = '';
  formType = 'Simple';
  formBranchId = '';
  formMenuSectionId = '';
  formDescription = '';
  formSortOrder = 0;
  formLoading = false;
  formError = '';

  // Supporting data
  sections: MenuSectionDto[] = [];
  branches: BranchSummary[] = [];
  sectionsLoading = false;
  branchesLoading = false;

  // Filters
  filterType = '';
  filterSection = '';

  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');
  readonly isOwner = this.auth.getActiveRole() === 'Owner';

  readonly productTypes = [
    { value: 'Simple',       label: 'بسيط' },
    { value: 'VariantBased', label: 'بأحجام' },
    { value: 'Customizable', label: 'قابل للتخصيص' },
    { value: 'Bundle',       label: 'وجبة' }
  ];

  constructor(
    public auth: AuthService,
    private productService: ProductService,
    private menuSectionService: MenuSectionService,
    private branchService: BranchService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadSections();
    if (this.isOwnerOrManager) this.loadBranches();
  }

  // ── Data loading ──────────────────────────────────────────────────────────────

  loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.productService.getProducts().subscribe({
      next: (p) => { this.products = p; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل المنتجات'; this.loading = false; }
    });
  }

  loadSections(): void {
    this.sectionsLoading = true;
    this.menuSectionService.getSections().subscribe({
      next: (s) => { this.sections = s.filter(x => x.isActive); this.sectionsLoading = false; },
      error: () => { this.sectionsLoading = false; }
    });
  }

  private loadBranches(): void {
    this.branchesLoading = true;
    this.branchService.getBranches().subscribe({
      next: (b) => { this.branches = b.filter(x => x.isActive); this.branchesLoading = false; },
      error: () => { this.branchesLoading = false; }
    });
  }

  // ── Panel open/close ──────────────────────────────────────────────────────────

  openCreatePanel(): void {
    this.formName = '';
    this.formType = 'Simple';
    this.formMenuSectionId = '';
    this.formDescription = '';
    this.formSortOrder = 0;
    this.formError = '';
    this.formBranchId = this.branches.length === 1 ? this.branches[0].id : '';
    this.createPanelOpen = true;
  }

  closeCreatePanel(): void {
    this.createPanelOpen = false;
    this.formError = '';
  }

  // ── Create ────────────────────────────────────────────────────────────────────

  createProduct(): void {
    if (!this.formName.trim()) { this.formError = 'اسم المنتج مطلوب'; return; }
    this.formLoading = true;
    this.formError = '';

    const request: CreateProductRequest = {
      branchId:      this.isOwnerOrManager ? this.formBranchId || null : null,
      menuSectionId: this.formMenuSectionId || null,
      name:          this.formName.trim(),
      description:   this.formDescription.trim() || null,
      imageUrl:      null,
      type:          this.formType,
      sortOrder:     this.formSortOrder
    };

    this.productService.createProduct(request).subscribe({
      next: (result) => {
        this.formLoading = false;
        this.router.navigate(['/menu/products', result.id]);
      },
      error: (err) => {
        this.formError = err?.error?.message ?? 'حدث خطأ أثناء إنشاء المنتج';
        this.formLoading = false;
      }
    });
  }

  // ── Navigation ────────────────────────────────────────────────────────────────

  openProduct(id: string): void {
    this.router.navigate(['/menu/products', id]);
  }

  // ── Filtering ─────────────────────────────────────────────────────────────────

  get filteredProducts(): ProductSummaryDto[] {
    return this.products.filter(p => {
      if (this.filterType && p.type !== this.filterType) return false;
      if (this.filterSection && p.menuSectionId !== this.filterSection) return false;
      return true;
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  typeLabel(type: string): string {
    switch (type) {
      case 'Simple':       return 'بسيط';
      case 'VariantBased': return 'بأحجام';
      case 'Customizable': return 'قابل للتخصيص';
      case 'Bundle':       return 'وجبة';
      default:             return type;
    }
  }

  typeClass(type: string): string {
    switch (type) {
      case 'Simple':       return 'type-simple';
      case 'VariantBased': return 'type-variant';
      case 'Customizable': return 'type-customizable';
      case 'Bundle':       return 'type-bundle';
      default:             return '';
    }
  }
}
