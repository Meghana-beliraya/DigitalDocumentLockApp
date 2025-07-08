import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTachometerAlt, faUpload, faFolderOpen, faCog } from '@fortawesome/free-solid-svg-icons';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule, FormsModule],
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  faTachometerAlt = faTachometerAlt;
  faUpload = faUpload;
  faFolderOpen = faFolderOpen;
  faCog = faCog;
  isSidebarCollapsed = false;
  showLogoutConfirm = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

 ngOnInit() {}

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  logout() {
    this.showLogoutConfirm = true;
  }

  confirmLogout(confirm: boolean) {
    if (confirm) {
      this.authService.logout().subscribe({
        next: () => { // runs for 200 ok
          if (isPlatformBrowser(this.platformId)) {
            localStorage.clear();
          }
          this.router.navigate(['/welcome']);
        },
        error: (err) => {
          console.error('Logout failed:', err);
          if (isPlatformBrowser(this.platformId)) { //checks running in browser
            localStorage.clear();
          }
          this.router.navigate(['/welcome']);
        }
      });
    }

    this.showLogoutConfirm = false;
  }
}
