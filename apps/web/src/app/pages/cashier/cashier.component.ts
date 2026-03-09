import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { OrderService, OrderDto } from '../../core/services/order.service';

@Component({
  selector: 'app-cashier',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cashier.component.html',
  styleUrl: './cashier.component.scss'
})
export class CashierComponent implements OnInit {
  name = this.auth.getFullName();
  orders: OrderDto[] = [];
  loading = true;
  error = '';

  constructor(public auth: AuthService, private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';
    this.orderService.getOrders().subscribe({
      next: (data) => { this.orders = data; this.loading = false; },
      error: () => { this.error = 'فشل تحميل الطلبات'; this.loading = false; }
    });
  }

  acceptOrder(id: string): void {
    // TODO: call PATCH /orders/:id/accept
    console.log('accept', id);
    alert(`سيتم قبول الطلب ${id} قريباً`);
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      PendingAcceptance: 'انتظار القبول',
      Accepted: 'مقبول',
      Preparing: 'قيد التحضير',
      ReadyForDispatch: 'جاهز للإرسال',
      PendingHandover: 'انتظار التسليم',
      PickedUp: 'تم الاستلام',
      Delivered: 'تم التوصيل',
      Completed: 'مكتمل',
      Cancelled: 'ملغي'
    };
    return labels[status] ?? status;
  }

  typeLabel(type: string): string {
    const labels: Record<string, string> = {
      DineIn: 'داخل المطعم',
      Delivery: 'توصيل',
      Pickup: 'استلام'
    };
    return labels[type] ?? type;
  }
}
