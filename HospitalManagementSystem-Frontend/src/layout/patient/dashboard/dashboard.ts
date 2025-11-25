import { Component, inject, signal } from '@angular/core';
import { Topnavbar } from '../../topnavbar/topnavbar';
import { Sidebar } from '../../sidebar/sidebar';
import { Home } from '../../../home/home';
import { User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';

@Component({
  selector: 'app-dashboard',
  imports: [Topnavbar, Sidebar, Home],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  
})
export class Dashboard {

  protected user  = signal<User | null>(null );
  protected accountservice = inject(AccountService);
}
