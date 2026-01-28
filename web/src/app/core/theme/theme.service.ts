import { DOCUMENT } from '@angular/common';
import { effect, Injectable, signal, inject } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly storageKey = 'hh_theme_mode';
  private readonly document = inject(DOCUMENT);
  private readonly themeSignal = signal<ThemeMode>('light');

  readonly theme = this.themeSignal.asReadonly();

  constructor() {
    this.loadPreference();
    effect(() => {
      this.applyTheme();
    });
  }

  setTheme(theme: ThemeMode) {
    this.themeSignal.set(theme);
    try {
      localStorage.setItem(this.storageKey, theme);
    } catch {
      // Ignore storage errors (private mode, disabled storage).
    }
  }

  toggleTheme() {
    this.setTheme(this.themeSignal() === 'dark' ? 'light' : 'dark');
  }

  private loadPreference() {
    try {
      const stored = localStorage.getItem(this.storageKey);
      if (stored === 'light' || stored === 'dark') {
        this.themeSignal.set(stored);
      }
    } catch {
      // Ignore storage errors (private mode, disabled storage).
    }
  }

  private applyTheme() {
    const resolved = this.themeSignal();
    const body = this.document.body;

    body.classList.remove('theme-light', 'theme-dark');
    body.classList.add(`theme-${resolved}`);

    this.document.documentElement.style.colorScheme = resolved;
  }
}
