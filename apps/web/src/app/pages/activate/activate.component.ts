import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { SuccessModalComponent } from '../../shared/ui/success-modal/success-modal.component';

@Component({
  selector: 'app-activate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SuccessModalComponent],
  templateUrl: './activate.component.html',
  styleUrl: './activate.component.scss'
})
export class ActivateComponent implements OnInit {
  inviteToken = '';
  fullName = '';
  phone = '';
  password = '';
  confirmPassword = '';
  loading = false;
  error = '';
  tokenMissing = false;
  showPassword = false;
  showConfirmPassword = false;
  showSuccess = false;

  constructor(private auth: AuthService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.inviteToken = token;
    } else {
      this.tokenMissing = true;
    }
  }

  togglePassword(): void { this.showPassword = !this.showPassword; }
  toggleConfirmPassword(): void { this.showConfirmPassword = !this.showConfirmPassword; }

  submit(): void {
    this.error = '';

    if (!this.fullName.trim())               { this.error = 'أدخل اسمك الكامل'; return; }
    if (!this.phone.trim())                  { this.error = 'أدخل رقم هاتفك'; return; }
    if (this.password.length < 8)            { this.error = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'; return; }
    if (this.password !== this.confirmPassword) { this.error = 'كلمتا المرور غير متطابقتين'; return; }

    this.loading = true;
    this.auth.activateInvite({
      inviteToken: this.inviteToken,
      fullName: this.fullName.trim(),
      phoneNumber: this.phone.trim(),
      password: this.password
    }).subscribe({
      next: () => { this.loading = false; this.showSuccess = true; },
      error: () => {
        this.loading = false;
        this.error = 'رمز الدعوة غير صالح أو منتهي الصلاحية';
      }
    });
  }

  onSuccessProceed(): void { this.auth.redirectByRole(); }
}
