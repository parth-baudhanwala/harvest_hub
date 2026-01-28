import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterModule } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { BasketService } from '../../core/services/basket.service';
import { ShoppingCartItem } from '../../core/models/basket';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {
  constructor(
    public readonly basketService: BasketService,
    public readonly authService: AuthService
  ) {}

  updateQuantity(item: ShoppingCartItem, value: string) {
    const quantity = Number(value);
    if (Number.isNaN(quantity)) return;
    this.basketService.updateItem(item.productId, quantity);
  }

  removeItem(item: ShoppingCartItem) {
    this.basketService.removeItem(item.productId);
  }
}
