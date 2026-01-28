import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';

import { ApiService } from '../api/api.service';
import { Product } from '../models/product';
import { PaginatedResult } from '../models/pagination';

interface GetProductsResponse {
  products: PaginatedResult<Product>;
}

interface GetProductByIdResponse {
  product: Product;
}

interface GetProductsByCategoryResponse {
  products: Product[];
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  constructor(private readonly api: ApiService) {}

  getProducts(index = 0, size = 10) {
    return this.api
      .get<GetProductsResponse>('/catalog-service/products', {
        Index: Math.max(1, index + 1),
        Size: size
      })
      .pipe(map((response) => response.products));
  }

  getProductsByCategory(category: string) {
    return this.api
      .get<GetProductsByCategoryResponse>(`/catalog-service/products/category/${encodeURIComponent(category)}`)
      .pipe(map((response) => response.products));
  }

  getProductById(id: string) {
    return this.api
      .get<GetProductByIdResponse>(`/catalog-service/products/${id}`)
      .pipe(map((response) => response.product));
  }
}
