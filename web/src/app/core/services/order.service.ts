import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';

import { ApiService } from '../api/api.service';
import { Order } from '../models/order';
import { PaginatedResult } from '../models/pagination';

interface GetOrdersResponse {
  orders: PaginatedResult<Order>;
}

interface GetOrdersByCustomerResponse {
  orders: Order[];
}

interface UpdateOrderRequest {
  order: Order;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private readonly api: ApiService) {}

  getOrders(index = 0, size = 10) {
    return this.api
      .get<GetOrdersResponse>('/ordering-service/orders', { Index: index, Size: size })
      .pipe(map((response) => response.orders));
  }

  getOrdersByCustomer(customerId: string) {
    return this.api
      .get<GetOrdersByCustomerResponse>(`/ordering-service/orders/customer/${encodeURIComponent(customerId)}`)
      .pipe(map((response) => response.orders));
  }

  updateOrder(order: Order) {
    const payload: UpdateOrderRequest = { order };
    return this.api.put<{ isSuccess: boolean }>(`/ordering-service/orders`, payload);
  }

  deleteOrder(orderId: string) {
    return this.api.delete<{ isSuccess: boolean }>(
      `/ordering-service/orders/${encodeURIComponent(orderId)}`
    );
  }
}
