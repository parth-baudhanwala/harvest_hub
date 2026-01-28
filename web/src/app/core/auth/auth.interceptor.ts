import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { API_BASE_URL } from '../api/api-base-url.token';
import { AUTH_BASE_URL } from './auth-base-url.token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const apiBaseUrl = inject(API_BASE_URL);
  const authBaseUrl = inject(AUTH_BASE_URL);
  const shouldAttachCredentials =
    req.url.startsWith(apiBaseUrl) || req.url.startsWith(authBaseUrl);

  if (!shouldAttachCredentials) {
    return next(req);
  }

  return next(req.clone({ withCredentials: true }));
};
