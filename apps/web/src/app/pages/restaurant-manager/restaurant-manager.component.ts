import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardLayoutComponent } from '../../shared/ui/dashboard-layout/dashboard-layout.component';
import { AuthService } from '../../core/services/auth.service';

interface StatCard {
  icon: string;
  label: string;
  value: number | string;
  accent: string;
}

interface StaffMember {
  name: string;
  role: string;
  phone: string;
  status: 'active' | 'pending';
}

@Component({
  selector: 'app-restaurant-manager',
  standalone: true,
  imports: [CommonModule, DashboardLayoutComponent],
  templateUrl: './restaurant-manager.component.html',
  styleUrl: './restaurant-manager.component.scss'
})
export class RestaurantManagerComponent {
  name = this.auth.getFullName();

  readonly stats: StatCard[] = [
    { icon: '🏪', label: 'الفروع',          value: 3,  accent: '#C4762A' },
    { icon: '👥', label: 'الموظفون',        value: 14, accent: '#3B82F6' },
    { icon: '🔥', label: 'طلبات نشطة',     value: 7,  accent: '#8B5CF6' },
    { icon: '📋', label: 'طلبات اليوم',    value: 23, accent: '#10B981' }
  ];

  readonly staff: StaffMember[] = [
    { name: 'سارة العتيبي',  role: 'كاشير',    phone: '0501112233', status: 'active'  },
    { name: 'خالد المطيري',  role: 'منسّق',    phone: '0502223344', status: 'active'  },
    { name: 'نورة الشمري',   role: 'كاشير',    phone: '0503334455', status: 'pending' }
  ];

  constructor(public auth: AuthService, private router: Router) {}

  inviteStaff(): void {
    this.router.navigate(['/restaurant-manager/staff']);
  }
}
