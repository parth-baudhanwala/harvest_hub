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
  AdminAdminDialogComponent,
  AdminAdminDialogResult
} from './dialog/admin-admin-dialog.component';

@Component({
  selector: 'app-admin-admins',
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
  templateUrl: './admin-admins.component.html',
  styleUrl: './admin-admins.component.css'
})
export class AdminAdminsComponent {
  private readonly adminUsers = inject(AdminUserService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly admins = signal<AdminUser[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  readonly displayedColumns = ['userName', 'email', 'actions'];

  constructor() {
    this.loadAdmins();
  }

  loadAdmins() {
    this.loading.set(true);
    this.adminUsers
      .getAdmins()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (admins) => this.admins.set(admins),
        error: () => this.error.set('Unable to load admins.')
      });
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(AdminAdminDialogComponent, {
      width: '420px',
      data: { mode: 'create' as const }
    });

    dialogRef.afterClosed().subscribe((result?: AdminAdminDialogResult) => {
      if (!result) return;
      this.saveCreate(result);
    });
  }

  editAdmin(admin: AdminUser) {
    const dialogRef = this.dialog.open(AdminAdminDialogComponent, {
      width: '420px',
      data: { mode: 'edit' as const, admin }
    });

    dialogRef.afterClosed().subscribe((result?: AdminAdminDialogResult) => {
      if (!result) return;
      this.saveUpdate(admin.id, result);
    });
  }

  deleteAdmin(admin: AdminUser) {
    const confirmed = window.confirm(`Delete admin ${admin.userName}?`);
    if (!confirmed) return;

    this.saving.set(true);
    this.adminUsers
      .deleteAdmin(admin.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Admin deleted.', 'Dismiss', { duration: 2500 });
          this.loadAdmins();
        },
        error: () => this.snackBar.open('Delete failed.', 'Dismiss', { duration: 2500 })
      });
  }

  private saveCreate(result: AdminAdminDialogResult) {
    if (!result.password) {
      this.error.set('Password is required for new admins.');
      return;
    }

    this.saving.set(true);
    this.adminUsers
      .createAdmin({
        username: result.username,
        email: result.email,
        password: result.password
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Admin created.', 'Dismiss', { duration: 2500 });
          this.loadAdmins();
        },
        error: () => this.error.set('Unable to create admin.')
      });
  }

  private saveUpdate(id: string, result: AdminAdminDialogResult) {
    this.saving.set(true);
    this.adminUsers
      .updateAdmin(id, {
        username: result.username,
        email: result.email
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Admin updated.', 'Dismiss', { duration: 2500 });
          this.loadAdmins();
        },
        error: () => this.error.set('Unable to update admin.')
      });
  }
}
