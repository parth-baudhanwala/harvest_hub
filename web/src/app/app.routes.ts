import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
	{
		path: '',
		loadComponent: () =>
			import('./features/layout/layout.component').then((m) => m.LayoutComponent),
		children: [
			{ path: '', pathMatch: 'full', redirectTo: 'catalog' },
			{
				path: 'catalog',
				loadChildren: () =>
					import('./features/catalog/catalog.routes').then((m) => m.catalogRoutes)
			},
			{
				path: 'cart',
				loadChildren: () => import('./features/cart/cart.routes').then((m) => m.cartRoutes)
			},
			{
				path: 'checkout',
				canActivate: [authGuard],
				loadChildren: () =>
					import('./features/checkout/checkout.routes').then((m) => m.checkoutRoutes)
			},
			{
				path: 'orders',
				canActivate: [authGuard],
				loadChildren: () => import('./features/orders/orders.routes').then((m) => m.ordersRoutes)
			}
		]
	},
	{ path: '**', redirectTo: 'catalog' }
];
