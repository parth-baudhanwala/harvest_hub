import { Routes } from '@angular/router';

export const catalogRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./catalog-list.component').then((m) => m.CatalogListComponent)
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./product-detail.component').then((m) => m.ProductDetailComponent)
  }
];
