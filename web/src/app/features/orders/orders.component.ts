import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';

import { AuthService } from '../../core/auth/auth.service';
import { CatalogService } from '../../core/services/catalog.service';
import { Order } from '../../core/models/order';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatButtonModule
  ],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent {
  readonly orders = signal<Order[]>([]);
  readonly loading = signal(false);
  readonly productNames = signal<Record<string, string>>({});

  constructor(
    private readonly orderService: OrderService,
    private readonly catalogService: CatalogService,
    public readonly auth: AuthService
  ) {
    const customerId = this.auth.customerId();
    if (customerId) {
      this.fetchOrders(customerId);
    }
  }

  private fetchOrders(customerId: string) {
    this.loading.set(true);
    this.orderService
      .getOrdersByCustomer(customerId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((orders) => {
        this.orders.set(orders);
        this.loadProductNames(orders);
      });
  }

  getTotal(order: Order) {
    return order.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  getOrderItemTotal(price: number, quantity: number) {
    return price * quantity;
  }

  shortId(id: string) {
    return id ? `${id.slice(0, 8)}…${id.slice(-4)}` : '';
  }

  productName(productId: string) {
    return this.productNames()[productId] ?? this.shortId(productId);
  }

  private loadProductNames(orders: Order[]) {
    const ids = Array.from(
      new Set(orders.flatMap((order) => order.items.map((item) => item.productId)))
    );
    if (!ids.length) return;

    const requests = ids.map((id) =>
      this.catalogService.getProductById(id).pipe(
        map((product) => ({ id, name: product.name })),
        catchError(() => of({ id, name: this.shortId(id) }))
      )
    );

    forkJoin(requests).subscribe((results) => {
      const mapResult = results.reduce<Record<string, string>>((acc, item) => {
        acc[item.id] = item.name;
        return acc;
      }, {});
      this.productNames.set(mapResult);
    });
  }
}
