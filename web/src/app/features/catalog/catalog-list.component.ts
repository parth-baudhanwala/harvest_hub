import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule } from '@angular/router';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize, map, startWith, switchMap, tap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { BasketService } from '../../core/services/basket.service';
import { CatalogService } from '../../core/services/catalog.service';
import { Product } from '../../core/models/product';

@Component({
  selector: 'app-catalog-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './catalog-list.component.html',
  styleUrl: './catalog-list.component.css'
})
export class CatalogListComponent {
  readonly products = signal<Product[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly categories = signal<string[]>([]);
  readonly placeholderImage =
    'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="640" height="400"><rect width="100%" height="100%" fill="%23e5e7eb"/><text x="50%" y="50%" font-size="20" text-anchor="middle" fill="%236b7280" font-family="Arial">No Image</text></svg>';

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly categoryControl = new FormControl('', { nonNullable: true });
  private readonly pageChanges = new BehaviorSubject<{ index: number; size: number }>({
    index: 0,
    size: 10
  });

  constructor(
    private readonly catalogService: CatalogService,
    private readonly basketService: BasketService
  ) {
    const search$ = this.searchControl.valueChanges.pipe(
      startWith(this.searchControl.value),
      debounceTime(300),
      distinctUntilChanged()
    );

    const category$ = this.categoryControl.valueChanges.pipe(
      startWith(this.categoryControl.value),
      distinctUntilChanged(),
      tap((category) => {
        if (category) {
          this.pageIndex.set(0);
          this.pageChanges.next({ index: 0, size: this.pageSize() });
        }
      })
    );

    combineLatest([search$, category$, this.pageChanges])
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap(([search, category, page]) =>
          this.getCatalogRequest(category, page).pipe(
            map((response) => ({ response, search, category })),
            finalize(() => this.loading.set(false))
          )
        ),
        takeUntilDestroyed()
      )
      .subscribe(({ response, search, category }) => {
        const list: Product[] = Array.isArray(response) ? response : response.data;
        const filtered = search
          ? list.filter((item) => item.name.toLowerCase().includes(search.trim().toLowerCase()))
          : list;

        this.products.set(filtered);
        this.total.set(Array.isArray(response) ? filtered.length : response.count);

        if (!category) {
          const categorySet = new Set<string>(this.categories());
          list.forEach((product) => product.categories.forEach((c) => categorySet.add(c)));
          this.categories.set(Array.from(categorySet));
        }
      });
  }

  addToCart(product: Product) {
    this.basketService.addItem(product, 1);
  }

  onPageChange(event: PageEvent) {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.pageChanges.next({ index: event.pageIndex, size: event.pageSize });
  }

  private getCatalogRequest(
    category: string,
    page: { index: number; size: number }
  ): Observable<Product[] | { data: Product[]; count: number }> {
    return category
      ? this.catalogService.getProductsByCategory(category)
      : this.catalogService.getProducts(page.index, page.size);
  }

  onImageError(event: Event) {
    const element = event.target as HTMLImageElement;
    element.src = this.placeholderImage;
  }
}
