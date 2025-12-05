import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminDashboardService } from '../../core/services/admin-dashboard-service';
import { AdminOverview } from '../../types/admin-overview';

@Component({
  selector: 'app-superadmin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './superadmin-dashboard.html',
  styleUrl: './superadmin-dashboard.css',
})
export class SuperAdminDashboard implements OnInit {
  private service = inject(AdminDashboardService);
  overview = signal<AdminOverview | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  ngOnInit(): void {
    this.loadOverview();
  }

  private loadOverview(): void {
    this.isLoading.set(true);
    this.service.getOverview().subscribe({
      next: (data) => {
        this.overview.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Unable to load system overview');
        this.isLoading.set(false);
      }
    });
  }
}
