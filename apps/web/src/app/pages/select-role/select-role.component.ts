import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface RoleOption {
  value: string;
  label: string;
  icon: string;
  description: string;
}

const ROLE_META: Record<string, Omit<RoleOption, 'value'>> = {
  Owner:             { label: 'المالك',          icon: '🏢', description: 'إدارة المطعم والفروع والموظفين' },
  RestaurantManager: { label: 'مدير المطعم',      icon: '👔', description: 'إدارة الفروع والطلبات والموظفين' },
  BranchManager:     { label: 'مدير الفرع',       icon: '🏪', description: 'إدارة الطلبات وموظفي الفرع' },
  Cashier:           { label: 'كاشير',            icon: '💳', description: 'استقبال وإدارة الطلبات' },
  Coordinator:       { label: 'منسّق',            icon: '📋', description: 'تنسيق الطلبات والتوصيل' },
  Driver:            { label: 'سائق',             icon: '🚗', description: 'استلام وتوصيل الطلبات' }
};

@Component({
  selector: 'app-select-role',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './select-role.component.html',
  styleUrl: './select-role.component.scss'
})
export class SelectRoleComponent implements OnInit {
  roleOptions: RoleOption[] = [];
  name = this.auth.getFullName();

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    const roles = this.auth.getRoles();

    // If somehow they only have 0 or 1 role, skip this screen
    if (roles.length === 0) {
      this.router.navigate(['/login']);
      return;
    }
    if (roles.length === 1) {
      this.auth.setActiveRole(roles[0]);
      this.auth.navigateForRole(roles[0]);
      return;
    }

    this.roleOptions = roles.map(r => ({
      value: r,
      ...(ROLE_META[r] ?? { label: r, icon: '👤', description: '' })
    }));
  }

  select(role: string): void {
    this.auth.setActiveRole(role);
    this.auth.navigateForRole(role);
  }

  logout(): void {
    this.auth.logout();
  }
}
