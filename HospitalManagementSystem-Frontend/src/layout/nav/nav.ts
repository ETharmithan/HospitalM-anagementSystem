import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationStart, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth-service';
import { User } from '../../types/user';

import { getRoleDashboardRoute } from '../../core/utils/role-utils';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule,CommonModule, RouterModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  isMenuOpen = false;
  showUserMenu = false;
  query = '';
  private authService = inject(AuthService);
  currentUser: User | null = null;

  constructor(private router: Router) {
    // Close mobile menu on navigation
    this.router.events.subscribe(e => {
      if (e instanceof NavigationStart) {
        this.isMenuOpen = false;
        this.showUserMenu = false;
      }
    });

    // Subscribe to auth state changes
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', (e) => {
      const target = e.target as HTMLElement;
      if (!target.closest('.user-menu-container')) {
        this.showUserMenu = false;
      }
    });
  }

  getHomeRoute(): string {
    if (this.currentUser) {
      return getRoleDashboardRoute(this.currentUser.role);
    }
    return '/';
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
    this.showUserMenu = false; // Close user menu when mobile menu opens
  }

  closeMenu(): void {
    this.isMenuOpen = false;
    this.showUserMenu = false;
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
    this.isMenuOpen = false; // Close mobile menu when user menu opens
  }

  onSearch() {
    // Implement search navigation or emit event
    if (this.query?.trim()) {
      this.router.navigate(['/search'], { queryParams: { q: this.query } });
      this.query = '';
      this.closeMenu();
    }
  }

  logout(): void {
    this.authService.logout();
    this.closeMenu();
    this.showUserMenu = false;
  }

  onImageError(event: any): void {
    event.target.style.display = 'none';
  }

  getImageUrl(imageUrl?: string): string {
    if (!imageUrl) return '/assets/avatar-placeholder.svg';
    
    // If it's already a full URL, return as is
    if (imageUrl.startsWith('http')) {
      return imageUrl;
    }
    
    // If it starts with /uploads, it's already correct
    if (imageUrl.startsWith('/uploads')) {
      return `http://localhost:5245${imageUrl}`;
    }
    
    // If it starts with uploads/, add the leading slash
    if (imageUrl.startsWith('uploads/')) {
      return `http://localhost:5245/${imageUrl}`;
    }
    
    // Default: assume it's a relative path
    return `http://localhost:5245/${imageUrl}`;
  }

  get userInitials(): string {
    if (!this.currentUser?.displayName) return 'U';
    return this.currentUser.displayName
      .split(' ')
      .map((n: string) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

}
