import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  mobileOpen = false;
    isDark = false;
    search = '';

    toggleTheme() {
      this.isDark = !this.isDark;
      const html = document.documentElement;
      if (this.isDark) {
        html.setAttribute('data-theme', 'dark');
      } else {
        html.setAttribute('data-theme', 'light');
      }
    }

    onSearch() {
      // implement search routing or emit event
      console.log('search for', this.search);
      // e.g. this.router.navigate(['/search'], { queryParams: { q: this.search }});
    }

    logout() {
      // implement logout logic
      console.log('logout clicked');
    }
}
