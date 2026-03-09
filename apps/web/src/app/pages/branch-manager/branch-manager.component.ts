import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-branch-manager',
  standalone: true,
  templateUrl: './branch-manager.component.html',
  styleUrl: './branch-manager.component.scss'
})
export class BranchManagerComponent {
  name = this.auth.getFullName();
  constructor(public auth: AuthService) {}
}
