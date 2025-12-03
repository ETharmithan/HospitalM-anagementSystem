import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminDashboardService } from '../../core/services/admin-dashboard-service';
import { AdminOverview } from '../../types/admin-overview';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit {
  private service = inject(AdminDashboardService);
  overview = signal<AdminOverview | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  ngOnInit(): void {
    this.loadOverview();
  }

  private loadOverview(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.service.getOverview().subscribe({
      next: (data) => {
        this.overview.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error?.message ?? 'Unable to load overview.');
        this.isLoading.set(false);
      }
    });
  }
}
