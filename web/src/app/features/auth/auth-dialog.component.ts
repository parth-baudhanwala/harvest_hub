import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';

import { AuthApiService } from '../../core/auth/auth-api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-auth-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './auth-dialog.component.html',
  styleUrl: './auth-dialog.component.css'
})
export class AuthDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AuthDialogComponent>);
  private readonly auth = inject(AuthService);
  private readonly authApi = inject(AuthApiService);
  private readonly fb = inject(FormBuilder);

  readonly mode = signal<'login' | 'register'>('login');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  readonly registerForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  readonly title = computed(() =>
    this.mode() === 'login' ? 'Sign in' : 'Create account'
  );

  switchMode(mode: 'login' | 'register') {
    this.mode.set(mode);
    this.error.set(null);
  }

  submit() {
    this.error.set(null);
    if (this.mode() === 'login') {
      if (this.loginForm.invalid) return;
      const payload = this.loginForm.getRawValue();
      this.loading.set(true);
      this.authApi
        .login(payload)
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe({
          next: (response) => {
            this.auth.setSession({
              username: response.user.userName ?? response.user.email,
              customerId: response.user.id
            });
            this.dialogRef.close();
          },
          error: () => this.error.set('Invalid email or password.')
        });
    } else {
      if (this.registerForm.invalid) return;
      const payload = this.registerForm.getRawValue();
      this.loading.set(true);
      this.authApi
        .register(payload)
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe({
          next: () => {
            this.switchMode('login');
            this.loginForm.patchValue({ email: payload.email });
          },
          error: (err) =>
            this.error.set(err?.error?.message ?? 'Registration failed. Please try again.')
        });
    }
  }
}
