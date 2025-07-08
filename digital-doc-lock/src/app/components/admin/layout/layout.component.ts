import { Component } from '@angular/core';
import { faTachometerAlt, faUpload, faFolderOpen, faCog, faUsers } from '@fortawesome/free-solid-svg-icons';
import { Router } from '@angular/router'; //routes link
import { RouterModule } from '@angular/router'; //Angular routing directives 
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-layout',
  templateUrl: './layout.component.html',
  imports: [CommonModule, FontAwesomeModule, FormsModule, RouterModule],
  styleUrls: ['./layout.component.scss']
})
export class AdminLayoutComponent {
  constructor(private router: Router) {}

  isSidebarCollapsed = false;
  showLogoutConfirm = false;



  // Icons
  faTachometerAlt = faTachometerAlt;
  faUpload = faUpload;
  faFolderOpen = faFolderOpen;
  faCog = faCog;
  faUsers = faUsers;

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  logout() {
    this.showLogoutConfirm = true;
  }

  confirmLogout(confirmed: boolean) {
    if (confirmed) {
      this.router.navigate(['/welcome']);
    }
    this.showLogoutConfirm = false;
  }
}
