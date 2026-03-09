import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  fullName = '';
  phone = '';
  password = '';
  restaurantName = '';
  loading = false;
  error = '';

  constructor(private auth: AuthService) {}

  submit(): void {
    this.error = '';

    if (!this.fullName.trim())      { this.error = 'أدخل اسمك الكامل'; return; }
    if (!this.phone.trim())         { this.error = 'أدخل رقم هاتفك'; return; }
    if (this.password.length < 8)   { this.error = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'; return; }
    if (!this.restaurantName.trim()){ this.error = 'أدخل اسم مطعمك'; return; }

    this.loading = true;
    this.auth.registerOwner({
      fullName: this.fullName.trim(),
      phoneNumber: this.phone.trim(),
      password: this.password,
      restaurantName: this.restaurantName.trim()
    }).subscribe({
      next: () => { this.loading = false; this.auth.redirectByRole(); },
      error: (err) => {
        this.loading = false;
        this.error = err.status === 409
          ? 'رقم الهاتف مسجّل بالفعل'
          : 'حدث خطأ، حاول مجدداً';
      }
    });
  }
}
