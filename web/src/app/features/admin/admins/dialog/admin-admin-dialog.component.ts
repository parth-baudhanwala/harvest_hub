import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { AdminUser } from '../../../../core/services/admin-user.service';

export interface AdminAdminDialogData {
  mode: 'create' | 'edit';
  admin?: AdminUser;
}

export interface AdminAdminDialogResult {
  mode: 'create' | 'edit';
  username: string;
  email: string;
  password?: string;
}

@Component({
  selector: 'app-admin-admin-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './admin-admin-dialog.component.html',
  styleUrl: './admin-admin-dialog.component.css'
})
export class AdminAdminDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AdminAdminDialogComponent>);
  private readonly data = inject<AdminAdminDialogData>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  readonly mode = this.data.mode;
  readonly form = this.fb.nonNullable.group({
    username: [this.data.admin?.userName ?? '', [Validators.required, Validators.minLength(2)]],
    email: [this.data.admin?.email ?? '', [Validators.required, Validators.email]],
    password: ['', this.mode === 'create' ? [Validators.required, Validators.minLength(6)] : []]
  });

  get title() {
    return this.mode === 'create' ? 'Create admin' : 'Edit admin';
  }

  submit() {
    if (this.form.invalid) return;
    this.dialogRef.close({
      mode: this.mode,
      username: this.form.getRawValue().username,
      email: this.form.getRawValue().email,
      password: this.form.getRawValue().password
    } as AdminAdminDialogResult);
  }

  cancel() {
    this.dialogRef.close();
  }
}
