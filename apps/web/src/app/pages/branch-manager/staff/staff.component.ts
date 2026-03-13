import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InviteService, CreateInviteResponse } from '../../../core/services/invite.service';
import { AuthService } from '../../../core/services/auth.service';

interface RoleOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff.component.html',
  styleUrl: './staff.component.scss'
})
export class StaffComponent {
  phoneNumber = '';
  selectedRole = '';
  loading = false;
  error = '';
  result: CreateInviteResponse | null = null;
  copied = false;

  // Role hierarchy: lower number = higher authority. Owner excluded — it is system-only.
  private readonly allRoles: RoleOption[] = [
    { value: 'RestaurantManager', label: 'مدير مطعم' },
    { value: 'BranchManager',     label: 'مدير فرع' },
    { value: 'Cashier',           label: 'كاشير' },
    { value: 'Coordinator',       label: 'منسّق' },
    { value: 'Driver',            label: 'سائق' }
  ];

  private readonly roleLevel: Record<string, number> = {
    Owner: 0, RestaurantManager: 1, BranchManager: 2,
    Cashier: 3, Coordinator: 3, Driver: 3
  };

  /** Only roles strictly below the caller's level are shown */
  get roles(): RoleOption[] {
    const callerLevel = this.roleLevel[this.auth.getActiveRole() ?? ''] ?? 99;
    return this.allRoles.filter(r => (this.roleLevel[r.value] ?? 99) > callerLevel);
  }

  constructor(
    private inviteService: InviteService,
    public auth: AuthService
  ) {}

  get canSubmit(): boolean {
    return !!this.phoneNumber.trim() && !!this.selectedRole && !this.loading;
  }

  sendInvite(): void {
    if (!this.canSubmit) return;
    this.loading = true;
    this.error = '';
    this.result = null;

    const branchId = localStorage.getItem('branchId') || null;

    this.inviteService.createInvite({
      phoneNumber: this.phoneNumber.trim(),
      role: this.selectedRole,
      branchId
    }).subscribe({
      next: (res) => { this.result = res; this.loading = false; },
      error: (err) => {
        this.error = err?.error?.message ?? 'حدث خطأ، يرجى المحاولة مرة أخرى';
        this.loading = false;
      }
    });
  }

  copyLink(): void {
    if (!this.result) return;
    navigator.clipboard.writeText(this.result.activationLink).then(() => {
      this.copied = true;
      setTimeout(() => (this.copied = false), 2500);
    });
  }

  reset(): void {
    this.phoneNumber = '';
    this.selectedRole = '';
    this.result = null;
    this.error = '';
    this.copied = false;
  }

  /**
   * Always returns to the dashboard of the currently committed activeRole.
   * Never hardcodes a route — the same component works for Owner, BranchManager, etc.
   */
  goBack(): void {
    this.auth.goToDashboard();
  }
}
