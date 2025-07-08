import { Component, OnInit } from '@angular/core'; // define component
import { CommonModule } from '@angular/common'; // Angular directives like *ngIf, *ngFor
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome'; // icons
import { faShieldAlt, faUsers, faFolderOpen, faUpload } from '@fortawesome/free-solid-svg-icons';
import { AdminService } from '../services/admin.service';  

interface ActivityLog {
  timestamp: string;
  message: string;
  userId: number;
  firstName: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule, FontAwesomeModule]
})
export class AdminDashboardComponent implements OnInit {
  totalUsers = 0;
  totalDocuments = 0;
  documentsToday = 0;
  activityLogs: ActivityLog[] = [];

  faShieldAlt = faShieldAlt;
  faUsers = faUsers;
  faFolderOpen = faFolderOpen;
  faUpload = faUpload;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  fetchDashboardData(): void {
    this.adminService.getAdminDashboardData().subscribe({
      next: (data) => {
        this.totalUsers = data.totalUsers;
        this.totalDocuments = data.totalDocuments;
        this.documentsToday = data.documentsUploadedToday;
        this.activityLogs = data.activityLogs;
      },
      error: (err) => {
        console.error('Error fetching dashboard data:', err);
      }
    });
  }
}
