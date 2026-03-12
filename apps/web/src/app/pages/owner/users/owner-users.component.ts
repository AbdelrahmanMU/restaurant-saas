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

@Component({
  selector: 'app-owner-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './owner-users.component.html',
  styleUrl: './owner-users.component.scss'
})
export class OwnerUsersComponent implements OnInit {
  users: UserSummary[] = [];
  loading = true;
  error = '';

  selectedDetail: UserDetail | null = null;
  detailLoading = false;
  addRoleValue = '';
  addRoleLoading = false;
  actionError = '';
  removingRoleId: string | null = null;

  readonly roleOptions: RoleOption[] = [
    { value: 'Cashier',           label: 'كاشير' },
    { value: 'Coordinator',       label: 'منسّق' },
    { value: 'BranchManager',     label: 'مدير فرع' },
    { value: 'RestaurantManager', label: 'مدير مطعم' },
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
      next: (users) => { this.users = users; this.loading = false; },
      error: () => { this.error = 'تعذّر تحميل قائمة الموظفين'; this.loading = false; }
    });
  }

  roleLabel(role: string): string {
    return ROLE_LABELS[role] ?? role;
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
      next: () => {
        this.removingRoleId = null;
        this.refreshDetail();
      },
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
        const idx = this.users.findIndex(x => x.id === userId);
        if (idx >= 0) {
          this.users[idx] = {
            ...this.users[idx],
            roles: detail.roleEntries.map(r => r.role)
          };
        }
      }
    });
  }
}
