import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { MenuSectionService, MenuSectionDto } from '../../../core/services/menu-section.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';

type PanelMode = 'create' | 'edit' | null;

@Component({
  selector: 'app-menu-sections',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-sections.component.html',
  styleUrl: './menu-sections.component.scss'
})
export class MenuSectionsComponent implements OnInit {
  sections: MenuSectionDto[] = [];
  loading = true;
  error = '';

  // ── Panel ────────────────────────────────────────────────────────────────────
  panelMode: PanelMode = null;
  selected: MenuSectionDto | null = null;

  // Form state
  formName = '';
  formDescription = '';
  formSortOrder = 0;
  formIsActive = true;
  formBranchId = '';
  formLoading = false;
  formError = '';

  // Branches for Owner/Manager branch selector
  branches: BranchSummary[] = [];
  branchesLoading = false;

  // Deactivate state
  deactivating = false;

  readonly isOwner = this.auth.getActiveRole() === 'Owner';
  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');

  constructor(
    public auth: AuthService,
    private menuSectionService: MenuSectionService,
    private branchService: BranchService
  ) {}

  ngOnInit(): void {
    this.loadSections();
    if (this.isOwnerOrManager) this.loadBranches();
  }

  // ── Data loading ─────────────────────────────────────────────────────────────

  loadSections(): void {
    this.loading = true;
    this.error = '';
    this.menuSectionService.getSections().subscribe({
      next: (secs) => { this.sections = secs; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل أقسام القائمة'; this.loading = false; }
    });
  }

  private loadBranches(): void {
    this.branchesLoading = true;
    this.branchService.getBranches().subscribe({
      next: (b) => { this.branches = b.filter(x => x.isActive); this.branchesLoading = false; },
      error: () => { this.branchesLoading = false; }
    });
  }

  // ── Panel open/close ─────────────────────────────────────────────────────────

  openCreate(): void {
    this.panelMode = 'create';
    this.selected = null;
    this.formName = '';
    this.formDescription = '';
    this.formSortOrder = 0;
    this.formIsActive = true;
    this.formBranchId = this.branches.length === 1 ? this.branches[0].id : '';
    this.formError = '';
  }

  openEdit(s: MenuSectionDto): void {
    this.panelMode = 'edit';
    this.selected = s;
    this.formName = s.name;
    this.formDescription = s.description ?? '';
    this.formSortOrder = s.sortOrder;
    this.formIsActive = s.isActive;
    this.formBranchId = s.branchId;
    this.formError = '';
  }

  closePanel(): void {
    this.panelMode = null;
    this.selected = null;
    this.formError = '';
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────────

  submit(): void {
    if (!this.formName.trim()) { this.formError = 'اسم القسم مطلوب'; return; }
    this.formLoading = true;
    this.formError = '';

    if (this.panelMode === 'create') {
      this.menuSectionService.createSection({
        branchId: this.isOwnerOrManager ? this.formBranchId || null : null,
        name: this.formName.trim(),
        description: this.formDescription.trim() || null,
        sortOrder: this.formSortOrder
      }).subscribe({
        next: (sec) => {
          this.sections = [...this.sections, sec]
            .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));
          this.formLoading = false;
          this.closePanel();
        },
        error: (err) => {
          const msg: string = err?.error?.message ?? '';
          this.formError = msg === 'DUPLICATE_NAME'
            ? 'يوجد قسم بهذا الاسم في هذا الفرع'
            : msg || 'حدث خطأ أثناء إنشاء القسم';
          this.formLoading = false;
        }
      });

    } else if (this.panelMode === 'edit' && this.selected) {
      this.menuSectionService.updateSection(this.selected.id, {
        name: this.formName.trim(),
        description: this.formDescription.trim() || null,
        sortOrder: this.formSortOrder,
        isActive: this.formIsActive
      }).subscribe({
        next: (sec) => {
          this.replaceSection(sec);
          this.selected = sec;
          this.formLoading = false;
        },
        error: (err) => {
          const msg: string = err?.error?.message ?? '';
          this.formError = msg === 'DUPLICATE_NAME'
            ? 'يوجد قسم بهذا الاسم في هذا الفرع'
            : msg || 'حدث خطأ أثناء تحديث القسم';
          this.formLoading = false;
        }
      });
    }
  }

  deactivate(): void {
    if (!this.selected) return;
    this.deactivating = true;
    this.formError = '';

    this.menuSectionService.deactivateSection(this.selected.id).subscribe({
      next: () => {
        const updated = { ...this.selected!, isActive: false };
        this.replaceSection(updated);
        this.selected = updated;
        this.formIsActive = false;
        this.deactivating = false;
      },
      error: (err) => {
        const msg: string = err?.error?.message ?? '';
        this.formError = msg || 'تعذّر إيقاف القسم';
        this.deactivating = false;
      }
    });
  }

  // ── Filtering ─────────────────────────────────────────────────────────────────

  get activeSections(): MenuSectionDto[] {
    return this.sections.filter(s => s.isActive);
  }

  get inactiveSections(): MenuSectionDto[] {
    return this.sections.filter(s => !s.isActive);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  branchName(branchId: string): string {
    return this.branches.find(b => b.id === branchId)?.name ?? '';
  }

  private replaceSection(updated: MenuSectionDto): void {
    const idx = this.sections.findIndex(s => s.id === updated.id);
    if (idx >= 0) this.sections[idx] = updated;
  }
}
