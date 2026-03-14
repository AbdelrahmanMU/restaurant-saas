import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { BranchService, BranchSummary } from '../../../core/services/branch.service';
import { UserManagementService, UserSummary } from '../../../core/services/user-management.service';

type PanelMode = 'create' | 'edit' | null;

@Component({
  selector: 'app-owner-branches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './owner-branches.component.html',
  styleUrl: './owner-branches.component.scss'
})
export class OwnerBranchesComponent implements OnInit {
  branches: BranchSummary[] = [];
  loading = true;
  error = '';

  // ── Panel state ──────────────────────────────────────────────────────────────
  panelMode: PanelMode = null;
  selected: BranchSummary | null = null;

  // Form fields (shared between create / edit)
  formName = '';
  formAddress = '';
  formIsActive = true;
  formLoading = false;
  formError = '';

  // Manager assignment (edit mode only)
  candidateUsers: UserSummary[] = [];
  usersLoading = false;
  selectedUserId = '';
  managerLoading = false;
  managerError = '';
  removingManager = false;

  readonly isOwner = this.auth.getActiveRole() === 'Owner';

  constructor(
    public auth: AuthService,
    private branchService: BranchService,
    private userService: UserManagementService
  ) {}

  ngOnInit(): void {
    this.loadBranches();
  }

  loadBranches(): void {
    this.loading = true;
    this.error = '';
    this.branchService.getBranches().subscribe({
      next: (b) => { this.branches = b; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل الفروع'; this.loading = false; }
    });
  }

  // ── Panel open/close ─────────────────────────────────────────────────────────

  openCreate(): void {
    this.panelMode = 'create';
    this.selected = null;
    this.formName = '';
    this.formAddress = '';
    this.formIsActive = true;
    this.formError = '';
    this.managerError = '';
  }

  openEdit(branch: BranchSummary): void {
    this.panelMode = 'edit';
    this.selected = branch;
    this.formName = branch.name;
    this.formAddress = branch.address;
    this.formIsActive = branch.isActive;
    this.formError = '';
    this.managerError = '';
    this.selectedUserId = '';
    this.loadCandidates(branch);
  }

  closePanel(): void {
    this.panelMode = null;
    this.selected = null;
    this.candidateUsers = [];
    this.formError = '';
    this.managerError = '';
  }

  // ── Branch CRUD ──────────────────────────────────────────────────────────────

  submit(): void {
    if (!this.formName.trim()) { this.formError = 'اسم الفرع مطلوب'; return; }
    this.formLoading = true;
    this.formError = '';

    if (this.panelMode === 'create') {
      this.branchService.createBranch({ name: this.formName.trim(), address: this.formAddress.trim() })
        .subscribe({
          next: (b) => {
            this.branches = [b, ...this.branches];
            this.formLoading = false;
            this.closePanel();
          },
          error: (err) => {
            this.formError = err?.error?.message ?? 'حدث خطأ أثناء إنشاء الفرع';
            this.formLoading = false;
          }
        });
    } else if (this.panelMode === 'edit' && this.selected) {
      this.branchService.updateBranch(this.selected.id, {
        name: this.formName.trim(),
        address: this.formAddress.trim(),
        isActive: this.formIsActive
      }).subscribe({
        next: (b) => {
          this.replaceBranch(b);
          this.selected = b;
          this.formLoading = false;
        },
        error: (err) => {
          this.formError = err?.error?.message ?? 'حدث خطأ أثناء تحديث الفرع';
          this.formLoading = false;
        }
      });
    }
  }

  // ── Manager assignment ────────────────────────────────────────────────────────

  private loadCandidates(branch: BranchSummary): void {
    this.usersLoading = true;
    this.userService.getUsers().subscribe({
      next: ({ manageable }) => {
        // Exclude the current manager from the candidate list
        this.candidateUsers = manageable.filter(u => u.id !== branch.manager?.userId);
        this.usersLoading = false;
      },
      error: () => { this.usersLoading = false; }
    });
  }

  assignManager(): void {
    if (!this.selected || !this.selectedUserId) return;
    this.managerLoading = true;
    this.managerError = '';

    this.branchService.assignManager(this.selected.id, this.selectedUserId).subscribe({
      next: (b) => {
        this.replaceBranch(b);
        this.selected = b;
        this.selectedUserId = '';
        // Refresh candidates (exclude new manager)
        this.loadCandidates(b);
        this.managerLoading = false;
      },
      error: (err) => {
        this.managerError = err?.error?.message ?? 'حدث خطأ أثناء تعيين المدير';
        this.managerLoading = false;
      }
    });
  }

  removeManager(): void {
    if (!this.selected) return;
    this.removingManager = true;
    this.managerError = '';

    this.branchService.removeManager(this.selected.id).subscribe({
      next: () => {
        const updated = { ...this.selected!, manager: null };
        this.replaceBranch(updated);
        this.selected = updated;
        this.loadCandidates(updated);
        this.removingManager = false;
      },
      error: (err) => {
        this.managerError = err?.error?.message ?? 'حدث خطأ أثناء إزالة المدير';
        this.removingManager = false;
      }
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────────────────

  private replaceBranch(updated: BranchSummary): void {
    const idx = this.branches.findIndex(b => b.id === updated.id);
    if (idx >= 0) this.branches[idx] = updated;
  }
}
