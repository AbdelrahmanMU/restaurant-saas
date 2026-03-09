import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { SuccessModalComponent } from '../../shared/ui/success-modal/success-modal.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SuccessModalComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  phone = '';
  password = '';
  loading = false;
  error = '';
  showPassword = false;
  showSuccess = false;

  constructor(private auth: AuthService) {}

  togglePassword(): void { this.showPassword = !this.showPassword; }

  submit(): void {
    this.error = '';

    if (!this.phone.trim()) { this.error = 'أدخل رقم هاتفك'; return; }
    if (!this.password)     { this.error = 'أدخل كلمة المرور'; return; }

    this.loading = true;
    this.auth.login(this.phone.trim(), this.password).subscribe({
      next: () => { this.loading = false; this.showSuccess = true; },
      error: (err) => {
        this.loading = false;
        this.error = err.status === 401
          ? 'رقم الهاتف أو كلمة المرور غير صحيحة'
          : 'حدث خطأ، حاول مجدداً';
      }
    });
  }

  onSuccessProceed(): void { this.auth.redirectByRole(); }
}
