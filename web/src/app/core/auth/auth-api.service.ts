import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { AUTH_BASE_URL } from './auth-base-url.token';

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: {
    id: string;
    userName: string;
    email: string;
  };
}

export interface MeResponse {
  id: string;
  userName: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(AUTH_BASE_URL);

  register(payload: RegisterRequest) {
    return this.http.post<{ message: string }>(`${this.baseUrl}/api/auth/register`, payload);
  }

  login(payload: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, payload);
  }

  adminLogin(payload: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/api/admin/login`, payload);
  }

  me() {
    return this.http.get<MeResponse>(`${this.baseUrl}/api/auth/me`);
  }

  logout() {
    return this.http.post<void>(`${this.baseUrl}/api/auth/logout`, {});
  }

  adminLogout() {
    return this.http.post<void>(`${this.baseUrl}/api/admin/logout`, {});
  }
}
