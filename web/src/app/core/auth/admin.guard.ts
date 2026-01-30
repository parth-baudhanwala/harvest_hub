import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from './auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const checkAccess = () => (auth.isAuthenticated() && auth.isAdmin()
    ? true
    : router.createUrlTree(['/admin/login']));

  if (auth.sessionChecked()) {
    return checkAccess();
  }

  return auth.refreshSession().pipe(
    map(() => checkAccess()),
    catchError(() => of(router.createUrlTree(['/admin/login'])))
  );
};
