import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service'; 

@Component({
  standalone: true,
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class SignupComponent {
  signupForm: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';
  formErrorMessage: string = '';
  submitted = false;

  constructor(
    private fb: FormBuilder, 
    private router: Router,
    private authService: AuthService
  ) {
    this.signupForm = this.fb.group(
      {
        firstName: ['', Validators.required],
        lastName: [''],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [
          Validators.required,
          Validators.pattern(/^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$/)
        ]],
        confirmPassword: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator }
    );
  }


  get password(): AbstractControl | null {
    return this.signupForm.get('password');
  }

  get confirmPassword(): AbstractControl | null {
    return this.signupForm.get('confirmPassword');
  }

  passwordMatchValidator(formGroup: AbstractControl): ValidationErrors | null {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;

    if (!password || !confirmPassword) return null;

    return password === confirmPassword ? null : { mismatch: true };
  }

  onSubmit() {
    this.submitted = true;
    this.formErrorMessage = '';  // Clear  previous form-level error

    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();  //  Triggers all field validations
      this.formErrorMessage = 'Please fill out all fields correctly.';
      return;
    }

    const formData = this.signupForm.getRawValue();

    const user = {
      firstName: formData.firstName,
      lastName: formData.lastName,
      email: formData.email,
      password: formData.password,
      isActive: true,
      isAdmin: false,
    };

    this.authService.register(user).subscribe(
      (response: any) => {
        this.successMessage = response?.message || 'Registration successful!';
        this.errorMessage = '';
        this.formErrorMessage = '';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      (error) => {
        this.successMessage = '';
        this.formErrorMessage = '';
        switch (error.status) {
          case 0:
            this.errorMessage = 'Cannot connect to the server. Please check your internet connection or try again later.';
            break;
          case 400:
            this.errorMessage = error.error?.message || 'Invalid request. Please check the entered data.';
            break;
          case 401:
            this.errorMessage = 'Unauthorized. Please check your credentials.';
            break;
          case 403:
            this.errorMessage = 'Access denied. You are not allowed to register as an admin.';
            break;
          case 409:
            this.errorMessage = 'This email is already registered. Please use another one or login.';
            break;
          case 500:
            this.errorMessage = 'Internal server error. Please try again later.';
            break;
          default:
            this.errorMessage = error.error?.message || 'An unexpected error occurred. Please try again.';
            break;
        }
      }
    );
  }
}
