import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  phone = '';
  password = '';
  loading = false;
  error = '';

  constructor(private auth: AuthService) {}

  submit(): void {
    this.error = '';

    if (!this.phone.trim()) { this.error = 'أدخل رقم هاتفك'; return; }
    if (!this.password)     { this.error = 'أدخل كلمة المرور'; return; }

    this.loading = true;
    this.auth.login(this.phone.trim(), this.password).subscribe({
      next: () => { this.loading = false; this.auth.redirectByRole(); },
      error: (err) => {
        this.loading = false;
        this.error = err.status === 401
          ? 'رقم الهاتف أو كلمة المرور غير صحيحة'
          : 'حدث خطأ، حاول مجدداً';
      }
    });
  }
}
