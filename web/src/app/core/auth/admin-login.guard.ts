import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from './auth.service';

export const adminLoginGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const routeIfAdmin = () =>
    auth.isAuthenticated() && auth.isAdmin()
      ? router.createUrlTree(['/admin/dashboard'])
      : true;

  if (auth.sessionChecked()) {
    return routeIfAdmin();
  }

  return true;
};
