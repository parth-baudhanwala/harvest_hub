import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs/operators';

import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models/order';
import {
  AdminOrderDialogComponent,
  AdminOrderDialogResult
} from './dialog/admin-order-dialog.component';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './admin-orders.component.html',
  styleUrl: './admin-orders.component.css'
})
export class AdminOrdersComponent {
  private readonly ordersService = inject(OrderService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly orders = signal<Order[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  readonly statusLabelMap: Record<number, string> = {
    1: 'Draft',
    2: 'Pending',
    3: 'Completed',
    4: 'Cancelled'
  };

  readonly displayedColumns = ['name', 'customer', 'status', 'total', 'actions'];

  constructor() {
    this.loadOrders();
  }

  loadOrders(pageIndex = this.pageIndex(), pageSize = this.pageSize()) {
    this.loading.set(true);
    this.ordersService
      .getOrders(pageIndex, pageSize)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          this.orders.set(result.data);
          this.pageIndex.set(result.index - 1);
          this.pageSize.set(result.size);
          this.totalCount.set(result.count);
        },
        error: () => this.error.set('Unable to load orders.')
      });
  }

  handlePage(event: PageEvent) {
    this.loadOrders(event.pageIndex, event.pageSize);
  }

  editOrder(order: Order) {
    const dialogRef = this.dialog.open(AdminOrderDialogComponent, {
      width: '420px',
      data: { order }
    });

    dialogRef.afterClosed().subscribe((result?: AdminOrderDialogResult) => {
      if (!result) return;
      const updatedOrder: Order = { ...order, status: result.status };
      this.saveUpdate(updatedOrder);
    });
  }

  deleteOrder(order: Order) {
    const confirmed = window.confirm(`Delete order ${order.name}?`);
    if (!confirmed) return;

    this.saving.set(true);
    this.ordersService
      .deleteOrder(order.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Order deleted.', 'Dismiss', { duration: 2500 });
          this.loadOrders();
        },
        error: () => this.snackBar.open('Delete failed.', 'Dismiss', { duration: 2500 })
      });
  }

  getTotal(order: Order) {
    return order.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  statusLabel(status: string | number) {
    const numeric = this.normalizeStatus(status);
    return this.statusLabelMap[numeric] ?? `${status}`;
  }

  private normalizeStatus(status: string | number) {
    if (typeof status === 'number') return status;
    const numeric = Number(status);
    return Number.isNaN(numeric) ? 2 : numeric;
  }

  private saveUpdate(order: Order) {
    this.saving.set(true);
    this.ordersService
      .updateOrder(order)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Order updated.', 'Dismiss', { duration: 2500 });
          this.loadOrders();
        },
        error: () => this.error.set('Unable to update order.')
      });
  }
}
