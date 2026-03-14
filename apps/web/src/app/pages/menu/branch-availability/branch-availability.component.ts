import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { BranchAvailabilityService, BranchVariantDto } from '../../../core/services/branch-availability.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';

@Component({
  selector: 'app-branch-availability',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './branch-availability.component.html',
  styleUrl: './branch-availability.component.scss'
})
export class BranchAvailabilityComponent implements OnInit {
  items: BranchVariantDto[] = [];
  loading = true;
  error = '';

  selectedBranchId = '';
  branches: BranchSummary[] = [];
  branchesLoading = false;

  editingId: string | null = null;
  editIsAvailable = true;
  editPriceOverride: number | null = null;
  editLoading = false;
  editError = '';

  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');

  constructor(
    public auth: AuthService,
    private availabilityService: BranchAvailabilityService,
    private branchService: BranchService
  ) {}

  ngOnInit(): void {
    if (this.isOwnerOrManager) {
      this.loadBranches();
    } else {
      this.loadAvailability();
    }
  }

  // ── Data loading ──────────────────────────────────────────────────────────────

  private loadBranches(): void {
    this.branchesLoading = true;
    this.branchService.getBranches().subscribe({
      next: (b) => {
        this.branches = b.filter(x => x.isActive);
        if (this.branches.length === 1) {
          this.selectedBranchId = this.branches[0].id;
        }
        this.branchesLoading = false;
        this.loadAvailability();
      },
      error: () => {
        this.branchesLoading = false;
        this.loading = false;
      }
    });
  }

  loadAvailability(): void {
    this.loading = true;
    this.error = '';
    const branchId = this.isOwnerOrManager ? this.selectedBranchId || undefined : undefined;
    this.availabilityService.getAvailability(branchId).subscribe({
      next: (items) => { this.items = items; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل بيانات التوفر'; this.loading = false; }
    });
  }

  onBranchChange(): void {
    this.editingId = null;
    this.editError = '';
    this.loadAvailability();
  }

  // ── Edit ─────────────────────────────────────────────────────────────────────

  startEdit(item: BranchVariantDto): void {
    this.editingId = item.productVariantId;
    this.editIsAvailable = item.isAvailable;
    this.editPriceOverride = item.priceOverride;
    this.editError = '';
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editError = '';
  }

  saveEdit(item: BranchVariantDto): void {
    this.editLoading = true;
    this.editError = '';

    const branchId = this.isOwnerOrManager ? this.selectedBranchId || undefined : undefined;
    this.availabilityService.upsert(
      item.productVariantId,
      { isAvailable: this.editIsAvailable, priceOverride: this.editPriceOverride },
      branchId
    ).subscribe({
      next: (updated) => {
        const idx = this.items.findIndex(x => x.productVariantId === updated.productVariantId);
        if (idx >= 0) this.items[idx] = updated;
        this.editLoading = false;
        this.editingId = null;
      },
      error: (err) => {
        this.editError = err?.error?.message ?? 'حدث خطأ أثناء الحفظ';
        this.editLoading = false;
      }
    });
  }

  // ── Sorted list ───────────────────────────────────────────────────────────────

  get filteredItems(): BranchVariantDto[] {
    return [...this.items].sort((a, b) => {
      const nameCompare = a.productName.localeCompare(b.productName);
      if (nameCompare !== 0) return nameCompare;
      return a.variantName.localeCompare(b.variantName);
    });
  }
}
