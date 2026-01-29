import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { Order } from '../../../../core/models/order';

export interface AdminOrderDialogData {
  order: Order;
}

export interface AdminOrderDialogResult {
  status: number;
}

@Component({
  selector: 'app-admin-order-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './admin-order-dialog.component.html',
  styleUrl: './admin-order-dialog.component.css'
})
export class AdminOrderDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AdminOrderDialogComponent>);
  private readonly data = inject<AdminOrderDialogData>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  readonly statusOptions = [
    { value: 1, label: 'Draft' },
    { value: 2, label: 'Pending' },
    { value: 3, label: 'Completed' },
    { value: 4, label: 'Cancelled' }
  ];

  readonly form = this.fb.nonNullable.group({
    status: [this.normalizeStatus(this.data.order.status), [Validators.required]]
  });

  get orderName() {
    return this.data.order.name;
  }

  submit() {
    if (this.form.invalid) return;
    this.dialogRef.close({ status: Number(this.form.getRawValue().status) } as AdminOrderDialogResult);
  }

  cancel() {
    this.dialogRef.close();
  }

  private normalizeStatus(status: string | number) {
    if (typeof status === 'number') return status;
    const numeric = Number(status);
    return Number.isNaN(numeric) ? 2 : numeric;
  }
}
