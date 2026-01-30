import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { AdminUser } from '../../../../core/services/admin-user.service';

export interface AdminUserDialogData {
  user: AdminUser;
}

export interface AdminUserDialogResult {
  username: string;
  email: string;
}

@Component({
  selector: 'app-admin-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './admin-user-dialog.component.html',
  styleUrl: './admin-user-dialog.component.css'
})
export class AdminUserDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AdminUserDialogComponent>);
  private readonly data = inject<AdminUserDialogData>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    username: [this.data.user.userName, [Validators.required, Validators.minLength(2)]],
    email: [this.data.user.email, [Validators.required, Validators.email]]
  });

  submit() {
    if (this.form.invalid) return;
    this.dialogRef.close({
      username: this.form.getRawValue().username,
      email: this.form.getRawValue().email
    } as AdminUserDialogResult);
  }

  cancel() {
    this.dialogRef.close();
  }
}
