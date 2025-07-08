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

        if (response && response.token) {
          this.authService.saveToken(response.token);
          localStorage.setItem('isAdmin', response.isAdmin.toString());

          this.errorMessage = '';

          if (response.isAdmin) {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/user/dashboard']);
          }
        } else {
          this.errorMessage = 'Invalid response from server.';
        }
      },
      error: (err) => {
        this.loading = false;
        console.error('Login failed:', err);

        if (err.status === 403 && err.error?.message) {
          this.errorMessage = err.error.message;
        } else if (err.status === 401 && err.error?.message) {
          this.errorMessage = err.error.message;
        } else if (err.error?.message) {
          this.errorMessage = err.error.message;
        } else {
          this.errorMessage = 'Login failed. Please try again later.';
        }
      }
    });
  }
}
