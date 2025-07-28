import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; 
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string = '';
  loading: boolean = false;
  submitted: boolean = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onInputChange(controlName: string): void {
    const control = this.loginForm.get(controlName);
    if (control && control.value === '') {
      control.markAsTouched();
      control.updateValueAndValidity();
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';

    if (this.loginForm.invalid) {
      return;
    }

    const credentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.loading = true;

    this.authService.login(credentials).subscribe({
      next: (response: any) => {
        this.loading = false;

        const accessToken = response.token;
        const refreshToken = response.refreshToken;

        if (accessToken && refreshToken) {
          this.authService.saveToken(accessToken);
          this.authService.saveRefreshToken(refreshToken);
          localStorage.setItem('isAdmin', response.isAdmin.toString());

          this.errorMessage = '';

          const targetRoute = response.isAdmin ? '/admin/dashboard' : '/user/dashboard';
          this.router.navigate([targetRoute]);
        } else {
          this.errorMessage = 'Invalid response from server.';
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Login failed:', err);

        if (err.status === 403 || err.status === 401) {
          this.errorMessage = err.error?.message || 'Authentication failed.';
        } else {
          this.errorMessage = 'Login failed. Please try again later.';
        }
      }
    });
  }
}
