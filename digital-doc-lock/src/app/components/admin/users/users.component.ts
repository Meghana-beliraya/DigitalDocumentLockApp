import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { HttpClientModule } from '@angular/common/http'; 

interface UserSummary {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  lastLogin: string;
  totalDocumentsUploaded: number;
}

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
  standalone: true,
  imports: [CommonModule, HttpClientModule],
})
export class UsersComponent implements OnInit {
  users: UserSummary[] = [];
  selectedUser: UserSummary | null = null;
  showModal = false;

  // Confirmation box state
  showConfirmationBox = false;
  confirmationMessage = '';
  targetUser: UserSummary | null = null;

  constructor(private http: HttpClient, private authService: AuthService) {}

  ngOnInit(): void {
    this.fetchUsers();
  }

  fetchUsers(): void {
    const token = this.authService.getToken();
    if (!token) return;

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http
      .get<UserSummary[]>('http://localhost:5138/api/Admin/all-users-summary', { headers })
      .subscribe({
        next: (res) => (this.users = res),
        error: (err) => console.error('Fetch users failed:', err),
      });
  }

  getFullName(user: UserSummary): string {
    return `${user.firstName} ${user.lastName}`;
  }

  getStatus(user: UserSummary): string {
    return user.isActive ? 'Active' : 'Inactive';
  }

  onView(user: UserSummary) {
    this.selectedUser = user;
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.selectedUser = null;
  }

  // Called when toggle button clicked
  onToggleStatus(user: UserSummary) {
    this.targetUser = user;
    this.confirmationMessage = user.isActive
      ? `Are you sure you want to deactivate ${this.getFullName(user)}?`
      : `Are you sure you want to activate ${this.getFullName(user)}?`;
    this.showConfirmationBox = true;
  }

  confirmToggleStatus(confirm: boolean) {
    if (!confirm || !this.targetUser) {
      this.showConfirmationBox = false;
      return;
    }

    const token = this.authService.getToken();
    if (!token) return;

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    const url = `http://localhost:5138/api/Admin/toggle-user-status/${this.targetUser.userId}`;

    this.http.put(url, {}, { headers }).subscribe({
      next: () => {
        this.targetUser!.isActive = !this.targetUser!.isActive;
        this.showConfirmationBox = false;
      },
      error: (err) => {
        console.error('Failed to toggle status:', err);
        this.showConfirmationBox = false;
      },
    });
  }
}
