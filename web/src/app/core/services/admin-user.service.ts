import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { AUTH_BASE_URL } from '../auth/auth-base-url.token';

export interface AdminUser {
  id: string;
  userName: string;
  email: string;
  roles: string[];
}

export interface UpdateUserRequest {
  username: string;
  email: string;
}

export interface CreateAdminRequest {
  username: string;
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(AUTH_BASE_URL);

  getUsers() {
    return this.http.get<AdminUser[]>(`${this.baseUrl}/api/admin/users`);
  }

  updateUser(id: string, payload: UpdateUserRequest) {
    return this.http.put<{ id: string; userName: string; email: string }>(
      `${this.baseUrl}/api/admin/users/${encodeURIComponent(id)}`,
      payload
    );
  }

  deleteUser(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/users/${encodeURIComponent(id)}`);
  }

  getAdmins() {
    return this.http.get<AdminUser[]>(`${this.baseUrl}/api/admin/admins`);
  }

  createAdmin(payload: CreateAdminRequest) {
    return this.http.post<{ id: string; userName: string; email: string }>(
      `${this.baseUrl}/api/admin/admins`,
      payload
    );
  }

  updateAdmin(id: string, payload: UpdateUserRequest) {
    return this.http.put<{ id: string; userName: string; email: string }>(
      `${this.baseUrl}/api/admin/admins/${encodeURIComponent(id)}`,
      payload
    );
  }

  deleteAdmin(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/api/admin/admins/${encodeURIComponent(id)}`);
  }
}
