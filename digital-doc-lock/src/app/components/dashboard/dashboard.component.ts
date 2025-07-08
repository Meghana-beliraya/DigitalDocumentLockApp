import { Component, OnInit } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, DashboardData } from '../../services/dashboard.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { AuthService } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';

interface Profile {
  firstName: string;
  lastName: string;
  email: string;
  profileImageUrl?: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [NgIf, NgFor, FormsModule, NgClass],
})
export class DashboardComponent implements OnInit {
  userName = '';
  total_file = 0;
  no_of_docs = 0;
  no_of_pdf = 0;
  recentActivities: string[] = [];

  showProfile = false;

  profile: Profile = {
    firstName: '',
    lastName: '',
    email: ''
  };

  profileImageUrl: SafeUrl | string = 'assets/profile.png';
  oldPassword = '';
  newPassword = '';
  confirmPassword = '';
  errorMessage: string = '';

  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  showToast = false;

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private profileService: ProfileService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
    this.loadProfile();
  }

  loadDashboardData() {
    this.dashboardService.getDashboardData().subscribe({
      next: (data: DashboardData) => {
        this.total_file = data.total_file;
        this.no_of_docs = data.no_of_docs;
        this.no_of_pdf = data.no_of_pdf;
        this.recentActivities = data.recentActivities;
      },
      error: (err) => console.error('Error loading dashboard data:', err),
    });
  }

loadProfile(): void {
  this.profileService.getProfile().subscribe({
    next: (data: Profile) => {
      this.profile.firstName = data.firstName;
      this.profile.lastName = data.lastName;
      this.profile.email = data.email;
      this.profileImageUrl = this.sanitizer.bypassSecurityTrustUrl(data.profileImageUrl!);
      this.userName = `${data.firstName} ${data.lastName}`;

      this.authService.storeUserProfile(data.firstName, data.email);
    },
    error: (err) => console.error('Error loading profile:', err),
  });
}


  toggleProfile() {
    this.showProfile = !this.showProfile;
    if (this.showProfile) {
      this.loadProfile();
      this.oldPassword = '';
      this.newPassword = '';
      this.confirmPassword = '';
    }
  }

  isStrongPassword(password: string): boolean {
    const strongRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
    return strongRegex.test(password);
  }

  saveProfile() {
    this.errorMessage = '';

    if (!this.oldPassword) {
      this.errorMessage = 'Please enter your current password to update profile.';
      return;
    }

    if (this.newPassword && !this.isStrongPassword(this.newPassword)) {
      this.errorMessage = 'New password must be strong: 8+ chars, uppercase, lowercase, number, special character.';
      return;
    }

    if (this.newPassword && this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'New password and confirm password do not match.';
      return;
    }

    const payload = {
      firstName: this.profile.firstName,
      lastName: this.profile.lastName,
      email: this.profile.email,
      oldPassword: this.oldPassword,
      newPassword: this.newPassword || '',
    };

    this.profileService.updateProfile(payload).subscribe({
      next: () => {
        this.userName = `${this.profile.firstName} ${this.profile.lastName}`;
        localStorage.setItem('firstName', this.profile.firstName);
        this.oldPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.errorMessage = '';
        this.showProfile = false;
        this.showToastMessage('Profile updated successfully.', 'success');
      },
      error: (err) => {
        console.error('Error updating profile:', err);
        const backendMessage = err.error?.message?.toLowerCase();

        if (err.status === 401 || (backendMessage && backendMessage.includes('incorrect password'))) {
          this.errorMessage = 'Incorrect current password. Please try again.';
        } else if (err.status === 400 && backendMessage?.includes('invalid input')) {
          this.errorMessage = 'Invalid profile data. Please check the inputs.';
        } else {
          this.errorMessage = backendMessage || 'Failed to update profile. Please try again later.';
        }
        this.showToastMessage('Failed to update profile.', 'error');
      },
    });
  }

  onFileSelected(event: any): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

  if (!allowedTypes.includes(file.type)) {
    this.showToastMessage('Only image files (JPEG, PNG, GIF, WEBP) are allowed.', 'error');
    return;
  }

  // UI preview logic (can stay in component)
  const reader = new FileReader();
  reader.onload = () => {
    this.profileImageUrl = reader.result as string;
  };
  reader.readAsDataURL(file);

  // Upload to server
  this.profileService.uploadProfileImage(file).subscribe({
    next: (res) => {
      this.profileImageUrl = this.sanitizer.bypassSecurityTrustUrl(res.profileImageUrl); // already formatted by service
      this.loadProfile(); // Refresh local state
      this.showToastMessage('Profile image updated!', 'success');
    },
    error: (err) => {
      console.error('Error uploading profile image:', err);
      this.showToastMessage('Failed to upload profile image.', 'error');
    },
  });
}

  showToastMessage(message: string, type: 'success' | 'error') {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;

    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }
}
