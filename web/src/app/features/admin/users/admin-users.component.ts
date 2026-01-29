import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs/operators';

import { AdminUser, AdminUserService } from '../../../core/services/admin-user.service';
import {
  AdminUserDialogComponent,
  AdminUserDialogResult
} from './dialog/admin-user-dialog.component';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent {
  private readonly adminUsers = inject(AdminUserService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly users = signal<AdminUser[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly displayedColumns = ['userName', 'email', 'roles', 'actions'];

  constructor() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading.set(true);
    this.adminUsers
      .getUsers()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (users) => this.users.set(users),
        error: () => this.error.set('Unable to load users.')
      });
  }

  editUser(user: AdminUser) {
    const dialogRef = this.dialog.open(AdminUserDialogComponent, {
      width: '420px',
      data: { user }
    });

    dialogRef.afterClosed().subscribe((result?: AdminUserDialogResult) => {
      if (!result) return;
      this.saveUpdate(user.id, result);
    });
  }

  deleteUser(user: AdminUser) {
    const confirmed = window.confirm(`Delete user ${user.userName}?`);
    if (!confirmed) return;

    this.saving.set(true);
    this.adminUsers
      .deleteUser(user.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('User deleted.', 'Dismiss', { duration: 2500 });
          this.loadUsers();
        },
        error: () => this.snackBar.open('Delete failed.', 'Dismiss', { duration: 2500 })
      });
  }

  private saveUpdate(id: string, result: AdminUserDialogResult) {
    this.saving.set(true);
    this.adminUsers
      .updateUser(id, {
        username: result.username,
        email: result.email
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('User updated.', 'Dismiss', { duration: 2500 });
          this.loadUsers();
        },
        error: () => this.error.set('Unable to update user.')
      });
  }
}
