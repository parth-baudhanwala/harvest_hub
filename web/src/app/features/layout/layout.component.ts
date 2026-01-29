import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AuthApiService } from '../../core/auth/auth-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { BasketService } from '../../core/services/basket.service';
import { ThemeService } from '../../core/theme/theme.service';
import { AuthDialogComponent } from '../auth/auth-dialog.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatDialogModule
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent {
  private readonly dialog = inject(MatDialog);
  private readonly authApi = inject(AuthApiService);

  readonly auth = inject(AuthService);
  readonly basket = inject(BasketService);
  readonly theme = inject(ThemeService);
  readonly cartCount = computed(() =>
    (this.basket.basket()?.items ?? []).reduce((sum, item) => sum + item.quantity, 0)
  );
  readonly currentYear = new Date().getFullYear();

  openAuthDialog() {
    this.dialog.open(AuthDialogComponent, {
      width: '420px'
    });
  }

  signOut() {
    this.authApi.logout().subscribe({
      next: () => this.auth.clearSession(),
      error: () => this.auth.clearSession()
    });
  }
}
