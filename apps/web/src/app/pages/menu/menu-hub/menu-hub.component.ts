import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-menu-hub',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './menu-hub.component.html',
  styleUrl: './menu-hub.component.scss'
})
export class MenuHubComponent {
  readonly isOwnerOrManager = ['Owner', 'RestaurantManager'].includes(this.auth.getActiveRole() ?? '');
  constructor(public auth: AuthService) {}
}
