import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { PreloadAllModules, provideRouter, withPreloading } from '@angular/router';

import { routes } from './app.routes';
import { API_BASE_URL } from './core/api/api-base-url.token';
import { AUTH_BASE_URL } from './core/auth/auth-base-url.token';
import { authInterceptor } from './core/auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withPreloading(PreloadAllModules)),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: API_BASE_URL,
      useFactory: () => localStorage.getItem('hh_api_base_url') ?? 'http://localhost:6004'
    },
    {
      provide: AUTH_BASE_URL,
      useFactory: () => localStorage.getItem('hh_auth_base_url') ?? 'http://localhost:6005'
    }
  ]
};
