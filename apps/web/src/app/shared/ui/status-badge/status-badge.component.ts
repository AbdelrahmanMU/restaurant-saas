import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  @Input() status = '';

  get label(): string {
    const labels: Record<string, string> = {
      PendingAcceptance: 'بانتظار القبول',
      Accepted: 'مقبول',
      Preparing: 'قيد التحضير',
      ReadyForDispatch: 'جاهز للإرسال',
      PendingHandover: 'بانتظار التسليم',
      PickedUp: 'تم الاستلام',
      Delivered: 'تم التوصيل',
      Completed: 'مكتمل',
      Cancelled: 'ملغي',
    };
    return labels[this.status] ?? this.status;
  }

  get cssClass(): string {
    const classes: Record<string, string> = {
      PendingAcceptance: 'pending',
      Accepted: 'accepted',
      Preparing: 'preparing',
      ReadyForDispatch: 'ready',
      PendingHandover: 'ready',
      PickedUp: 'delivered',
      Delivered: 'delivered',
      Completed: 'delivered',
      Cancelled: 'cancelled',
    };
    return classes[this.status] ?? 'pending';
  }
}
