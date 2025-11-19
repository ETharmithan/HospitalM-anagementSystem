import { CommonModule } from '@angular/common';
import { Component, HostListener} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationStart, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule,CommonModule, RouterModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
 isMenuOpen = false;
  query = '';

  constructor(private router: Router) {
    // Close mobile menu on navigation
    this.router.events.subscribe(e => {
      if (e instanceof NavigationStart) {
        this.isMenuOpen = false;
      }
    });
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  onSearch() {
    // Implement search navigation or emit event
    if (this.query?.trim()) {
      this.router.navigate(['/search'], { queryParams: { q: this.query } });
      this.query = '';
      this.closeMenu();
    }
  }

}
