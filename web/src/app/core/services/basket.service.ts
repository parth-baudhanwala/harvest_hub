import { Injectable, effect, signal } from '@angular/core';
import { catchError, finalize, of, tap } from 'rxjs';

import { ApiService } from '../api/api.service';
import { AuthService } from '../auth/auth.service';
import { CheckoutPayload, ShoppingCart, ShoppingCartItem } from '../models/basket';
import { Product } from '../models/product';

interface GetBasketResponse {
  cart: ShoppingCart;
}

interface StoreBasketResponse {
  isSuccess: boolean;
}

interface CheckoutBasketResponse {
  isSuccess: boolean;
}

@Injectable({ providedIn: 'root' })
export class BasketService {
  private readonly basketSignal = signal<ShoppingCart | null>(null);
  private readonly loadingSignal = signal(false);

  readonly basket = this.basketSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();

  constructor(
    private readonly api: ApiService,
    private readonly auth: AuthService
  ) {
    effect(() => {
      if (!this.auth.sessionChecked()) return;
      const username = this.auth.username();
      this.loadBasket(username ?? 'guest');
    });
  }

  loadBasket(username: string) {
    this.loadingSignal.set(true);
    if (this.auth.isAuthenticated()) {
      const guestBasket = this.readLocal('guest');
      this.api
        .get<GetBasketResponse>(`/basket-service/basket/${encodeURIComponent(username)}`)
        .pipe(
          tap((response) => {
            const merged = this.mergeBaskets(response.cart, guestBasket);
            this.setAndPersistLocal(merged);
            if (guestBasket?.items.length) {
              this.persistBasket(merged).subscribe();
              this.clearLocal('guest');
            }
          }),
          catchError(() => {
            const merged = this.mergeBaskets(this.createEmptyBasket(username), guestBasket);
            this.setAndPersistLocal(merged);
            if (guestBasket?.items.length) {
              this.persistBasket(merged).subscribe();
              this.clearLocal('guest');
            }
            return of(null);
          }),
          finalize(() => this.loadingSignal.set(false))
        )
        .subscribe();
    } else {
      const cached = this.readLocal(username) ?? this.createEmptyBasket(username);
      this.basketSignal.set(cached);
      this.loadingSignal.set(false);
    }
  }

  addItem(product: Product, quantity = 1) {
    const username = this.ensureUsername();
    if (!username) return;

    const current = this.basketSignal() ?? this.createEmptyBasket(username);
    const existing = current.items.find((item) => item.productId === product.id);

    if (existing) {
      existing.quantity += quantity;
    } else {
      current.items.push({
        productId: product.id,
        productName: product.name,
        price: product.price,
        quantity,
        description: product.description
      });
    }

    const updated = this.recalculate(current);
    this.setAndPersistLocal(updated);
    this.persistBasket(updated).subscribe();
  }

  updateItem(productId: string, quantity: number) {
    const username = this.ensureUsername();
    if (!username) return;

    const current = this.basketSignal() ?? this.createEmptyBasket(username);
    const item = current.items.find((i) => i.productId === productId);

    if (!item) return;

    item.quantity = Math.max(0, quantity);
    if (item.quantity === 0) {
      current.items = current.items.filter((i) => i.productId !== productId);
    }

    const updated = this.recalculate(current);
    this.setAndPersistLocal(updated);
    this.persistBasket(updated).subscribe();
  }

  removeItem(productId: string) {
    const username = this.ensureUsername();
    if (!username) return;

    const current = this.basketSignal() ?? this.createEmptyBasket(username);
    current.items = current.items.filter((item) => item.productId !== productId);
    const updated = this.recalculate(current);
    this.setAndPersistLocal(updated);
    this.persistBasket(updated).subscribe();
  }

  checkout(payload: CheckoutPayload) {
    return this.api
      .post<CheckoutBasketResponse>('/basket-service/basket/checkout', {
        checkout: payload
      })
      .pipe(
        tap((response) => {
          if (response.isSuccess) {
            this.basketSignal.set(this.createEmptyBasket(payload.username));
          }
        })
      );
  }

  private persistBasket(cart: ShoppingCart) {
    if (!this.auth.isAuthenticated()) {
      return of({ isSuccess: true });
    }

    return this.api
      .post<StoreBasketResponse>('/basket-service/basket', { cart })
      .pipe(catchError(() => of({ isSuccess: false })));
  }

  private ensureUsername() {
    return this.auth.username() ?? 'guest';
  }

  private createEmptyBasket(username: string): ShoppingCart {
    return {
      username,
      items: [],
      totalPrice: 0
    };
  }

  private recalculate(cart: ShoppingCart): ShoppingCart {
    const total = cart.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
    return { ...cart, totalPrice: total };
  }

  private mergeBaskets(base: ShoppingCart, incoming: ShoppingCart | null): ShoppingCart {
    if (!incoming?.items.length) {
      return this.recalculate({ ...base, items: [...base.items] });
    }

    const items = base.items.map((item) => ({ ...item }));
    for (const item of incoming.items) {
      const existing = items.find((current) => current.productId === item.productId);
      if (existing) {
        existing.quantity += item.quantity;
      } else {
        items.push({ ...item });
      }
    }

    return this.recalculate({ ...base, items });
  }

  private setAndPersistLocal(cart: ShoppingCart) {
    this.basketSignal.set(cart);
    localStorage.setItem(this.getLocalKey(cart.username), JSON.stringify(cart));
  }

  private readLocal(username: string): ShoppingCart | null {
    const raw = localStorage.getItem(this.getLocalKey(username));
    if (!raw) return null;
    try {
      return JSON.parse(raw) as ShoppingCart;
    } catch {
      return null;
    }
  }

  private getLocalKey(username: string) {
    return `hh_basket_${username}`;
  }

  private clearLocal(username: string) {
    localStorage.removeItem(this.getLocalKey(username));
  }
}
