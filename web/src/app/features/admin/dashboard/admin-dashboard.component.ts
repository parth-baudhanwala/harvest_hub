import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';

import { CatalogService } from '../../../core/services/catalog.service';
import { OrderService } from '../../../core/services/order.service';
import { AdminUserService } from '../../../core/services/admin-user.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent {
  private readonly catalogService = inject(CatalogService);
  private readonly orderService = inject(OrderService);
  private readonly adminUserService = inject(AdminUserService);

  readonly loading = signal(true);
  readonly productCount = signal(0);
  readonly orderCount = signal(0);
  readonly userCount = signal(0);
  readonly adminCount = signal(0);

  constructor() {
    this.loadCounts();
  }

  private loadCounts() {
    this.loading.set(true);

    const productRequest = this.catalogService.getProducts(0, 1);
    const orderRequest = this.orderService.getOrders(0, 1);
    const userRequest = this.adminUserService.getUsers();
    const adminRequest = this.adminUserService.getAdmins();

    productRequest.pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (result) => this.productCount.set(result.count)
    });

    orderRequest.subscribe({
      next: (result) => this.orderCount.set(result.count)
    });

    userRequest.subscribe({
      next: (users) => this.userCount.set(users.length)
    });

    adminRequest.subscribe({
      next: (admins) => this.adminCount.set(admins.length)
    });
  }
}
