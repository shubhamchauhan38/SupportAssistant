
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Auth } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  private fb = inject(FormBuilder);
  private authService = inject(Auth);
  private router = inject(Router);

  isLoading = false;
  errorMessage = '';

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  onLogin(): void {

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: (response) => {
        this.isLoading = false;

        if (response.role === 'Employee') {
          this.router.navigate(['/employee/dashboard']);
        }
        else if (response.role === 'SupportEngineer') {
          this.router.navigate(['/engineer/dashboard']);
        }
        else {
          this.errorMessage = 'Unknown user role.';
        }
      },
      error: (error) => {
        console.error('Login failed:', error);

        this.errorMessage =
          error.error?.message || 'Invalid email or password.';

        this.isLoading = false;
      }
    });
  }
}
