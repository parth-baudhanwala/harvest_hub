import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.sessionChecked()) {
    return auth.isAuthenticated() ? true : router.createUrlTree(['/']);
  }

  return auth.refreshSession().pipe(
    map(() => (auth.isAuthenticated() ? true : router.createUrlTree(['/']))),
    catchError(() => of(router.createUrlTree(['/'])))
  );
};
