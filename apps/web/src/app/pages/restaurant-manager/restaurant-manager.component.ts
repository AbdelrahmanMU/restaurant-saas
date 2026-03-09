import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-restaurant-manager',
  standalone: true,
  templateUrl: './restaurant-manager.component.html',
  styleUrl: './restaurant-manager.component.scss'
})
export class RestaurantManagerComponent {
  name = this.auth.getFullName();
  constructor(public auth: AuthService) {}
}
