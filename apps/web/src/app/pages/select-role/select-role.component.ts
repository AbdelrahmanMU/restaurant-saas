import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  name = this.auth.getFullName();
  roleOptions: RoleOption[] = [];

  constructor(public auth: AuthService) {}

  ngOnInit(): void {
    // Guard guarantees roles.length > 1 by the time we reach here
    this.roleOptions = this.auth.getRoles().map(r => ({
      value: r,
      ...(ROLE_META[r] ?? { label: r, icon: '👤', description: '' })
    }));
  }

  select(role: string): void {
    this.auth.setActiveRole(role);
    this.auth.navigateForRole(role);
  }
}
