import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-activate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './activate.component.html',
  styleUrl: './activate.component.scss'
})
export class ActivateComponent implements OnInit {
  inviteToken = '';
  fullName = '';
  phone = '';
  password = '';
  loading = false;
  error = '';
  tokenMissing = false;

  constructor(private auth: AuthService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.inviteToken = token;
    } else {
      this.tokenMissing = true;
    }
  }

  submit(): void {
    this.error = '';

    if (!this.fullName.trim())    { this.error = 'أدخل اسمك الكامل'; return; }
    if (!this.phone.trim())       { this.error = 'أدخل رقم هاتفك'; return; }
    if (this.password.length < 8) { this.error = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'; return; }

    this.loading = true;
    this.auth.activateInvite({
      inviteToken: this.inviteToken,
      fullName: this.fullName.trim(),
      phoneNumber: this.phone.trim(),
      password: this.password
    }).subscribe({
      next: () => { this.loading = false; this.auth.redirectByRole(); },
      error: () => {
        this.loading = false;
        this.error = 'رمز الدعوة غير صالح أو منتهي الصلاحية';
      }
    });
  }
}
