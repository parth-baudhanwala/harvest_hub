import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { Product } from '../../../../core/models/product';
import {
  CreateProductRequest,
  UpdateProductRequest
} from '../../../../core/services/catalog.service';

export interface AdminProductDialogData {
  mode: 'create' | 'edit';
  product?: Product;
}

export interface AdminProductDialogResult {
  mode: 'create' | 'edit';
  payload: CreateProductRequest | UpdateProductRequest;
}

@Component({
  selector: 'app-admin-product-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './admin-product-dialog.component.html',
  styleUrl: './admin-product-dialog.component.css'
})
export class AdminProductDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AdminProductDialogComponent>);
  private readonly data = inject<AdminProductDialogData>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  readonly mode = this.data.mode;
  readonly error = signal<string | null>(null);
  readonly imageBase64 = signal<string | null>(null);
  readonly maxFileSizeMb = 2;
  readonly allowedExtensions = ['jpg', 'jpeg', 'png', 'webp'];

  readonly form = this.fb.nonNullable.group({
    id: [this.data.product?.id ?? ''],
    name: [this.data.product?.name ?? '', [Validators.required, Validators.minLength(2)]],
    categories: [
      this.data.product?.categories?.join(', ') ?? '',
      [Validators.required]
    ],
    price: [this.data.product?.price ?? 0, [Validators.required, Validators.min(0)]],
    description: [
      this.data.product?.description ?? '',
      [Validators.required, Validators.minLength(5)]
    ],
    imageFile: [this.data.product?.imageFile ?? ''],
    imageFileName: [''],
    imageContentType: ['']
  });

  get title() {
    return this.mode === 'create' ? 'Create product' : 'Edit product';
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    const extension = file.name.split('.').pop()?.toLowerCase() ?? '';
    const sizeMb = file.size / (1024 * 1024);

    if (!this.allowedExtensions.includes(extension)) {
      this.error.set('Only JPG, PNG, or WEBP files are allowed.');
      return;
    }

    if (sizeMb > this.maxFileSizeMb) {
      this.error.set('Image size must be 2MB or less.');
      return;
    }

    this.form.patchValue({
      imageFileName: file.name,
      imageContentType: file.type
    });

    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result?.toString() ?? '';
      const base64 = result.includes(',') ? result.split(',')[1] : result;
      this.imageBase64.set(base64);
    };
    reader.readAsDataURL(file);
  }

  submit() {
    this.error.set(null);
    if (this.form.invalid) return;

    if (this.mode === 'create') {
      const imageBytes = this.imageBase64();
      if (!imageBytes) {
        this.error.set('Please upload a product image.');
        return;
      }

      const payload: CreateProductRequest = {
        name: this.form.getRawValue().name,
        categories: this.parseCategories(this.form.getRawValue().categories),
        description: this.form.getRawValue().description,
        imageFileName: this.form.getRawValue().imageFileName,
        imageBytes,
        imageContentType: this.form.getRawValue().imageContentType,
        price: Number(this.form.getRawValue().price)
      };

      this.dialogRef.close({ mode: 'create', payload } as AdminProductDialogResult);
      return;
    }

    const payload: UpdateProductRequest = {
      id: this.form.getRawValue().id,
      name: this.form.getRawValue().name,
      categories: this.parseCategories(this.form.getRawValue().categories),
      description: this.form.getRawValue().description,
      imageFile: this.form.getRawValue().imageFile,
      price: Number(this.form.getRawValue().price)
    };

    this.dialogRef.close({ mode: 'edit', payload } as AdminProductDialogResult);
  }

  cancel() {
    this.dialogRef.close();
  }

  private parseCategories(value: string) {
    return value
      .split(',')
      .map((item) => item.trim())
      .filter(Boolean);
  }
}
