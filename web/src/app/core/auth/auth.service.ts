import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, map, of, tap } from 'rxjs';

import { AuthApiService } from './auth-api.service';

export interface AuthSession {
  username: string;
  customerId?: string | null;
  roles?: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usernameSignal = signal<string | null>(null);
  private readonly customerIdSignal = signal<string | null>(null);
  private readonly rolesSignal = signal<string[]>([]);
  private readonly sessionCheckedSignal = signal(false);

  private readonly authApi = inject(AuthApiService);

  readonly username = this.usernameSignal.asReadonly();
  readonly customerId = this.customerIdSignal.asReadonly();
  readonly roles = this.rolesSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.usernameSignal());
  readonly isAdmin = computed(() => this.rolesSignal().includes('Admin'));
  readonly sessionChecked = this.sessionCheckedSignal.asReadonly();

  constructor() {}

  setSession(session: AuthSession) {
    this.usernameSignal.set(session.username.trim());
    this.customerIdSignal.set(session.customerId?.trim() ?? null);
    this.rolesSignal.set(session.roles ?? []);
    this.sessionCheckedSignal.set(true);
  }

  clearSession() {
    this.usernameSignal.set(null);
    this.customerIdSignal.set(null);
    this.rolesSignal.set([]);
    this.sessionCheckedSignal.set(true);
  }

  refreshSession() {
    return this.authApi.me().pipe(
      tap((user) => {
        this.usernameSignal.set(user.userName ?? user.email);
        this.customerIdSignal.set(user.id ?? null);
        this.rolesSignal.set(user.roles ?? []);
        this.sessionCheckedSignal.set(true);
      }),
      map(() => void 0),
      catchError(() => {
        this.clearSession();
        return of(void 0);
      })
    );
  }
}
