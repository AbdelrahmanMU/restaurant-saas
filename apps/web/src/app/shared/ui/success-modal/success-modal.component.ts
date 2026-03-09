import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-success-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './success-modal.component.html',
  styleUrl: './success-modal.component.scss'
})
export class SuccessModalComponent {
  @Input() icon = '✅';
  @Input() iconBg = '#D1FAE5';
  @Input() title = '';
  @Input() message = '';
  @Input() actionLabel = 'متابعة';
  @Output() proceed = new EventEmitter<void>();
}
