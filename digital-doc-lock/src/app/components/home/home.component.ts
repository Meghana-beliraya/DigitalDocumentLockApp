import { Component } from '@angular/core';
import { Router } from '@angular/router'; // navigate b/w the components

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  features = [
    { icon: '🔒', title: 'Secure Encryption', description: 'Protect your documents with AES encryption.' },
    { icon: '📂', title: 'Easy Upload', description: 'Upload and manage PDFs and Word files effortlessly.' },
    { icon: '🔑', title: 'Password Protection', description: 'Lock your files with a password for extra security.' },
    { icon: '👨‍💼', title: 'Admin Management', description: 'Admins can monitor and manage document security.' }
  ];

  constructor(private router: Router) {}

  goToLogin() {
    this.router.navigate(['/login']); //  Navigates to the Login page
  }

  goToSignup() {
    this.router.navigate(['/signup']); // Navigates to the Signup page
  }

  animateFeature(event: Event) {
    const card = event.currentTarget as HTMLElement;
    card.style.boxShadow = '0 10px 20px rgba(0, 233, 93, 0.4)';
  }

  resetFeature(event: Event) {
    const card = event.currentTarget as HTMLElement;
    card.style.boxShadow = '0 4px 10px rgba(0, 0, 0, 0.2)';
  }
}
