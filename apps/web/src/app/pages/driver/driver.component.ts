import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-driver',
  standalone: true,
  templateUrl: './driver.component.html',
  styleUrl: './driver.component.scss'
})
export class DriverComponent {
  name = this.auth.getFullName();
  constructor(public auth: AuthService) {}
}
