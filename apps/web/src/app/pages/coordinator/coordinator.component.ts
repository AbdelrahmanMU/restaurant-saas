import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-coordinator',
  standalone: true,
  templateUrl: './coordinator.component.html',
  styleUrl: './coordinator.component.scss'
})
export class CoordinatorComponent {
  name = this.auth.getFullName();
  constructor(public auth: AuthService) {}
}
