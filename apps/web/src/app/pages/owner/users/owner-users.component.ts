import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import {
  UserManagementService,
  UserSummary,
  UserDetail
} from '../../../core/services/user-management.service';

interface RoleOption {
  value: string;
  label: string;
}

const ROLE_LABELS: Record<string, string> = {
  Owner:             'مالك',
  RestaurantManager: 'مدير مطعم',
  BranchManager:     'مدير فرع',
  Cashier:           'كاشير',
  Coordinator:       'منسّق',
  Driver:            'سائق'
};

// Hierarchy level (lower = higher authority)
const ROLE_LEVEL: Record<string, number> = {
  Owner: 0, RestaurantManager: 1, BranchManager: 2,
  Cashier: 3, Coordinator: 3, Driver: 3
};

@Component({
  selector: 'app-owner-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './owner-users.component.html',
  styleUrl: './owner-users.component.scss'
})
export class OwnerUsersComponent implements OnInit {
  manageable: UserSummary[] = [];
  managers: UserSummary[] = [];
  loading = true;
  error = '';

  selectedDetail: UserDetail | null = null;
  detailLoading = false;
  addRoleValue = '';
  addRoleLoading = false;
  actionError = '';
  removingRoleId: string | null = null;

  // Owner role is intentionally absent — it is system-established only
  private readonly allRoleOptions: RoleOption[] = [
    { value: 'RestaurantManager', label: 'مدير مطعم' },
    { value: 'BranchManager',     label: 'مدير فرع' },
    { value: 'Cashier',           label: 'كاشير' },
    { value: 'Coordinator',       label: 'منسّق' },
    { value: 'Driver',            label: 'سائق' }
  ];

  constructor(
    public auth: AuthService,
    private userService: UserManagementService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.userService.getUsers().subscribe({
      next: ({ manageable, managers }) => {
        this.manageable = manageable;
        this.managers = managers;
        this.loading = false;
      },
      error: () => { this.error = 'تعذّر تحميل قائمة الموظفين'; this.loading = false; }
    });
  }

  roleLabel(role: string): string {
    return ROLE_LABELS[role] ?? role;
  }

  /** Roles the caller may assign — strictly below caller's level and not already held by the employee */
  get roleOptions(): RoleOption[] {
    const callerLevel = ROLE_LEVEL[this.auth.getActiveRole() ?? ''] ?? 99;
    const existing = new Set(this.selectedDetail?.roleEntries.map(r => r.role) ?? []);
    return this.allRoleOptions.filter(r =>
      (ROLE_LEVEL[r.value] ?? 99) > callerLevel && !existing.has(r.value)
    );
  }

  /** Whether the caller can remove a given role value from a subordinate's profile */
  canRemoveRole(roleValue: string): boolean {
    if (roleValue === 'Owner') return false; // immutable system role
    const callerLevel = ROLE_LEVEL[this.auth.getActiveRole() ?? ''] ?? 99;
    return (ROLE_LEVEL[roleValue] ?? 99) > callerLevel;
  }

  openUser(user: UserSummary): void {
    this.selectedDetail = null;
    this.addRoleValue = '';
    this.actionError = '';
    this.detailLoading = true;

    this.userService.getUser(user.id).subscribe({
      next: (detail) => { this.selectedDetail = detail; this.detailLoading = false; },
      error: () => { this.detailLoading = false; this.error = 'تعذّر تحميل بيانات الموظف'; }
    });
  }

  closePanel(): void {
    this.selectedDetail = null;
    this.addRoleValue = '';
    this.actionError = '';
  }

  addRole(): void {
    if (!this.selectedDetail || !this.addRoleValue) return;
    this.addRoleLoading = true;
    this.actionError = '';

    this.userService.addRole(this.selectedDetail.id, { role: this.addRoleValue }).subscribe({
      next: () => {
        this.addRoleLoading = false;
        this.addRoleValue = '';
        this.refreshDetail();
      },
      error: (err) => {
        this.actionError = err?.error?.message ?? 'حدث خطأ أثناء إضافة الدور';
        this.addRoleLoading = false;
      }
    });
  }

  removeRole(roleId: string): void {
    if (!this.selectedDetail) return;
    this.removingRoleId = roleId;
    this.actionError = '';

    this.userService.removeRole(this.selectedDetail.id, roleId).subscribe({
      next: () => { this.removingRoleId = null; this.refreshDetail(); },
      error: (err) => {
        this.actionError = err?.error?.message ?? 'تعذّر حذف الدور';
        this.removingRoleId = null;
      }
    });
  }

  private refreshDetail(): void {
    if (!this.selectedDetail) return;
    const userId = this.selectedDetail.id;
    this.userService.getUser(userId).subscribe({
      next: (detail) => {
        this.selectedDetail = detail;
        const idx = this.manageable.findIndex(x => x.id === userId);
        if (idx >= 0) {
          this.manageable[idx] = {
            ...this.manageable[idx],
            roles: detail.roleEntries.map(r => r.role)
          };
        }
      }
    });
  }
}
