import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import {
  ModifierGroupService,
  ModifierGroupDto,
  ModifierOptionDto
} from '../../../core/services/modifier-group.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';

type PanelMode = 'create' | 'edit' | null;
type OptionPanelMode = 'add' | 'edit' | null;

@Component({
  selector: 'app-modifier-groups',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modifier-groups.component.html',
  styleUrl: './modifier-groups.component.scss'
})
export class ModifierGroupsComponent implements OnInit {
  groups: ModifierGroupDto[] = [];
  loading = true;
  error = '';

  // ── Group panel ──────────────────────────────────────────────────────────────
  panelMode: PanelMode = null;
  selected: ModifierGroupDto | null = null;

  // Group form state
  formName = '';
  formSelectionType: 'Single' | 'Multiple' = 'Single';
  formIsRequired = false;
  formMinSelections = 0;
  formMaxSelections: number | null = null;
  formSortOrder = 0;
  formIsActive = true;
  formBranchId = '';
  formLoading = false;
  formError = '';

  // Branches
  branches: BranchSummary[] = [];
  branchesLoading = false;

  // Deactivate group
  deactivating = false;

  readonly isOwner = this.auth.getActiveRole() === 'Owner';
  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');

  // ── Option panel ─────────────────────────────────────────────────────────────
  optionPanelMode: OptionPanelMode = null;
  selectedOption: ModifierOptionDto | null = null;

  optFormName = '';
  optFormPriceDelta = 0;
  optFormIsDefault = false;
  optFormSortOrder = 0;
  optFormLoading = false;
  optFormError = '';

  // Deactivate option
  deactivatingOptionId: string | null = null;

  constructor(
    public auth: AuthService,
    private modifierGroupService: ModifierGroupService,
    private branchService: BranchService
  ) {}

  ngOnInit(): void {
    this.loadGroups();
    if (this.isOwnerOrManager) this.loadBranches();
  }

  // ── Data loading ─────────────────────────────────────────────────────────────

  loadGroups(): void {
    this.loading = true;
    this.error = '';
    this.modifierGroupService.getGroups().subscribe({
      next: (g) => { this.groups = g; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل مجموعات الإضافات'; this.loading = false; }
    });
  }

  private loadBranches(): void {
    this.branchesLoading = true;
    this.branchService.getBranches().subscribe({
      next: (b) => { this.branches = b.filter(x => x.isActive); this.branchesLoading = false; },
      error: () => { this.branchesLoading = false; }
    });
  }

  // ── Group panel open/close ────────────────────────────────────────────────────

  openCreate(): void {
    this.panelMode = 'create';
    this.selected = null;
    this.formName = '';
    this.formSelectionType = 'Single';
    this.formIsRequired = false;
    this.formMinSelections = 0;
    this.formMaxSelections = null;
    this.formSortOrder = 0;
    this.formIsActive = true;
    this.formBranchId = this.branches.length === 1 ? this.branches[0].id : '';
    this.formError = '';
    this.optionPanelMode = null;
    this.optFormError = '';
  }

  openEdit(group: ModifierGroupDto): void {
    this.panelMode = 'edit';
    this.selected = group;
    this.formName = group.name;
    this.formSelectionType = group.selectionType;
    this.formIsRequired = group.isRequired;
    this.formMinSelections = group.minSelections;
    this.formMaxSelections = group.maxSelections;
    this.formSortOrder = group.sortOrder;
    this.formIsActive = group.isActive;
    this.formBranchId = group.branchId;
    this.formError = '';
    this.optionPanelMode = null;
    this.optFormError = '';
  }

  closePanel(): void {
    this.panelMode = null;
    this.selected = null;
    this.formError = '';
    this.optionPanelMode = null;
    this.optFormError = '';
  }

  // ── Group CRUD ────────────────────────────────────────────────────────────────

  submit(): void {
    if (!this.formName.trim()) { this.formError = 'اسم المجموعة مطلوب'; return; }
    this.formLoading = true;
    this.formError = '';

    if (this.panelMode === 'create') {
      this.modifierGroupService.createGroup({
        branchId: this.isOwnerOrManager ? this.formBranchId || null : null,
        name: this.formName.trim(),
        selectionType: this.formSelectionType,
        isRequired: this.formIsRequired,
        minSelections: this.formMinSelections,
        maxSelections: this.formMaxSelections,
        sortOrder: this.formSortOrder
      }).subscribe({
        next: (g) => {
          this.groups = [...this.groups, g]
            .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));
          this.formLoading = false;
          this.closePanel();
        },
        error: (err) => {
          this.formError = err?.error?.message ?? 'حدث خطأ أثناء إنشاء المجموعة';
          this.formLoading = false;
        }
      });

    } else if (this.panelMode === 'edit' && this.selected) {
      this.modifierGroupService.updateGroup(this.selected.id, {
        name: this.formName.trim(),
        selectionType: this.formSelectionType,
        isRequired: this.formIsRequired,
        minSelections: this.formMinSelections,
        maxSelections: this.formMaxSelections,
        sortOrder: this.formSortOrder,
        isActive: this.formIsActive
      }).subscribe({
        next: (g) => {
          this.replaceGroup(g);
          this.selected = g;
          this.formLoading = false;
        },
        error: (err) => {
          this.formError = err?.error?.message ?? 'حدث خطأ أثناء تحديث المجموعة';
          this.formLoading = false;
        }
      });
    }
  }

  deactivate(): void {
    if (!this.selected) return;
    this.deactivating = true;
    this.formError = '';

    this.modifierGroupService.deactivateGroup(this.selected.id).subscribe({
      next: () => {
        const updated = { ...this.selected!, isActive: false };
        this.replaceGroup(updated);
        this.selected = updated;
        this.formIsActive = false;
        this.deactivating = false;
      },
      error: (err) => {
        const msg: string = err?.error?.message ?? '';
        this.formError = msg || 'تعذّر إيقاف المجموعة';
        this.deactivating = false;
      }
    });
  }

  // ── Filtering ─────────────────────────────────────────────────────────────────

  get activeGroups(): ModifierGroupDto[] {
    return this.groups.filter(g => g.isActive);
  }

  get inactiveGroups(): ModifierGroupDto[] {
    return this.groups.filter(g => !g.isActive);
  }

  get activeOptions(): ModifierOptionDto[] {
    return this.selected?.options.filter(o => o.isActive) ?? [];
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  branchName(branchId: string): string {
    return this.branches.find(b => b.id === branchId)?.name ?? '';
  }

  private replaceGroup(updated: ModifierGroupDto): void {
    const idx = this.groups.findIndex(g => g.id === updated.id);
    if (idx >= 0) this.groups[idx] = updated;
  }

  private replaceOption(updated: ModifierOptionDto): void {
    if (!this.selected) return;
    const idx = this.selected.options.findIndex(o => o.id === updated.id);
    if (idx >= 0) {
      this.selected = {
        ...this.selected,
        options: this.selected.options.map((o, i) => i === idx ? updated : o)
      };
      this.replaceGroup(this.selected);
    }
  }

  // ── Option panel open/close ───────────────────────────────────────────────────

  openAddOption(): void {
    this.optionPanelMode = 'add';
    this.selectedOption = null;
    this.optFormName = '';
    this.optFormPriceDelta = 0;
    this.optFormIsDefault = false;
    this.optFormSortOrder = 0;
    this.optFormError = '';
  }

  openEditOption(opt: ModifierOptionDto): void {
    this.optionPanelMode = 'edit';
    this.selectedOption = opt;
    this.optFormName = opt.name;
    this.optFormPriceDelta = opt.priceDelta;
    this.optFormIsDefault = opt.isDefault;
    this.optFormSortOrder = opt.sortOrder;
    this.optFormError = '';
  }

  closeOptionPanel(): void {
    this.optionPanelMode = null;
    this.selectedOption = null;
    this.optFormError = '';
  }

  // ── Option CRUD ───────────────────────────────────────────────────────────────

  submitOption(): void {
    if (!this.optFormName.trim()) { this.optFormError = 'اسم الخيار مطلوب'; return; }
    if (!this.selected) return;
    this.optFormLoading = true;
    this.optFormError = '';

    if (this.optionPanelMode === 'add') {
      this.modifierGroupService.addOption(this.selected.id, {
        name: this.optFormName.trim(),
        priceDelta: this.optFormPriceDelta,
        isDefault: this.optFormIsDefault,
        sortOrder: this.optFormSortOrder
      }).subscribe({
        next: (opt) => {
          this.selected = {
            ...this.selected!,
            options: [...this.selected!.options, opt]
              .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name))
          };
          this.replaceGroup(this.selected);
          this.optFormLoading = false;
          this.closeOptionPanel();
        },
        error: (err) => {
          this.optFormError = err?.error?.message ?? 'حدث خطأ أثناء إضافة الخيار';
          this.optFormLoading = false;
        }
      });

    } else if (this.optionPanelMode === 'edit' && this.selectedOption) {
      this.modifierGroupService.updateOption(this.selected.id, this.selectedOption.id, {
        name: this.optFormName.trim(),
        priceDelta: this.optFormPriceDelta,
        isDefault: this.optFormIsDefault,
        sortOrder: this.optFormSortOrder,
        isActive: true
      }).subscribe({
        next: (opt) => {
          this.replaceOption(opt);
          this.optFormLoading = false;
          this.closeOptionPanel();
        },
        error: (err) => {
          this.optFormError = err?.error?.message ?? 'حدث خطأ أثناء تحديث الخيار';
          this.optFormLoading = false;
        }
      });
    }
  }

  deactivateOption(opt: ModifierOptionDto): void {
    if (!this.selected) return;
    this.deactivatingOptionId = opt.id;

    this.modifierGroupService.deactivateOption(this.selected.id, opt.id).subscribe({
      next: () => {
        const updated = { ...opt, isActive: false };
        this.replaceOption(updated);
        this.deactivatingOptionId = null;
      },
      error: () => {
        this.deactivatingOptionId = null;
      }
    });
  }
}
