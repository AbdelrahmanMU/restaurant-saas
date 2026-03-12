import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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

  readonly roles: RoleOption[] = [
    { value: 'Cashier',           label: 'كاشير' },
    { value: 'Coordinator',       label: 'منسّق' },
    { value: 'BranchManager',     label: 'مدير فرع' },
    { value: 'RestaurantManager', label: 'مدير مطعم' },
    { value: 'Driver',            label: 'سائق' }
  ];

  constructor(
    private inviteService: InviteService,
    public auth: AuthService,
    private router: Router
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
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: (err) => {
        const msg = err?.error?.message;
        this.error = msg ?? 'حدث خطأ، يرجى المحاولة مرة أخرى';
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

  goBack(): void {
    this.router.navigate(['/branch-manager']);
  }
}
