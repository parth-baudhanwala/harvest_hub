import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs/operators';

import { CatalogService, CreateProductRequest, UpdateProductRequest } from '../../../core/services/catalog.service';
import { Product } from '../../../core/models/product';
import {
  AdminProductDialogComponent,
  AdminProductDialogResult
} from './dialog/admin-product-dialog.component';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.css'
})
export class AdminProductsComponent {
  private readonly catalog = inject(CatalogService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly products = signal<Product[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  readonly displayedColumns = ['name', 'price', 'categories', 'actions'];

  constructor() {
    this.loadProducts();
  }

  loadProducts(pageIndex = this.pageIndex(), pageSize = this.pageSize()) {
    this.loading.set(true);
    this.catalog
      .getProducts(pageIndex, pageSize)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          this.products.set(result.data);
          this.pageIndex.set(result.index - 1);
          this.pageSize.set(result.size);
          this.totalCount.set(result.count);
        },
        error: () => this.error.set('Unable to load products.')
      });
  }

  handlePage(event: PageEvent) {
    this.loadProducts(event.pageIndex, event.pageSize);
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(AdminProductDialogComponent, {
      width: '520px',
      data: { mode: 'create' as const }
    });

    dialogRef.afterClosed().subscribe((result?: AdminProductDialogResult) => {
      if (!result) return;
      const payload = result.payload as CreateProductRequest;
      this.saveCreate(payload);
    });
  }

  openEditDialog(product: Product) {
    const dialogRef = this.dialog.open(AdminProductDialogComponent, {
      width: '520px',
      data: { mode: 'edit' as const, product }
    });

    dialogRef.afterClosed().subscribe((result?: AdminProductDialogResult) => {
      if (!result) return;
      const payload = result.payload as UpdateProductRequest;
      this.saveUpdate(payload);
    });
  }

  deleteProduct(product: Product) {
    const confirmed = window.confirm(`Delete ${product.name}?`);
    if (!confirmed) return;

    this.saving.set(true);
    this.catalog
      .deleteProduct(product.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Product deleted.', 'Dismiss', { duration: 2500 });
          this.loadProducts();
        },
        error: () => this.snackBar.open('Delete failed.', 'Dismiss', { duration: 2500 })
      });
  }

  private saveCreate(payload: CreateProductRequest) {
    this.saving.set(true);
    this.catalog
      .createProduct(payload)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Product created.', 'Dismiss', { duration: 2500 });
          this.loadProducts();
        },
        error: () => this.snackBar.open('Unable to create product.', 'Dismiss', { duration: 2500 })
      });
  }

  private saveUpdate(payload: UpdateProductRequest) {
    this.saving.set(true);
    this.catalog
      .updateProduct(payload)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Product updated.', 'Dismiss', { duration: 2500 });
          this.loadProducts();
        },
        error: () => this.snackBar.open('Unable to update product.', 'Dismiss', { duration: 2500 })
      });
  }
}
