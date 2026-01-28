import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { BasketService } from '../../core/services/basket.service';
import { CatalogService } from '../../core/services/catalog.service';
import { Product } from '../../core/models/product';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent {
  readonly product = signal<Product | null>(null);
  readonly loading = signal(false);
  readonly placeholderImage =
    'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="640" height="400"><rect width="100%" height="100%" fill="%23e5e7eb"/><text x="50%" y="50%" font-size="20" text-anchor="middle" fill="%236b7280" font-family="Arial">No Image</text></svg>';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly catalogService: CatalogService,
    private readonly basketService: BasketService
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
    }
  }

  addToCart(product: Product) {
    this.basketService.addItem(product, 1);
  }

  private loadProduct(id: string) {
    this.loading.set(true);
    this.catalogService
      .getProductById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((product) => this.product.set(product));
  }

  onImageError(event: Event) {
    const element = event.target as HTMLImageElement;
    element.src = this.placeholderImage;
  }
}
