import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AuthService } from '../../core/auth/auth.service';
import { BasketService } from '../../core/services/basket.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    RouterModule
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    customerId: [''],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    emailAddress: ['', [Validators.required, Validators.email]],
    addressLine: ['', [Validators.required]],
    country: ['', [Validators.required]],
    state: ['', [Validators.required]],
    zipCode: ['', [Validators.required]],
    paymentMethod: [1, [Validators.required]],
    cardName: [''],
    cardNumber: [''],
    expirationMonth: [''],
    expirationYear: [''],
    cvv: ['']
  });

  readonly months = Array.from({ length: 12 }, (_, index) => String(index + 1).padStart(2, '0'));
  readonly years = Array.from({ length: 10 }, (_, index) => String(new Date().getFullYear() + index));

  constructor(
    private readonly auth: AuthService,
    private readonly basketService: BasketService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router
  ) {
    const customerId = this.auth.customerId();
    if (customerId) {
      this.form.patchValue({ customerId });
    }

    this.form
      .get('paymentMethod')
      ?.valueChanges.pipe(takeUntilDestroyed())
      .subscribe((value) => this.toggleCardValidators(value === 1));

    this.toggleCardValidators(this.form.getRawValue().paymentMethod === 1);
  }

  submit() {
    if (this.form.invalid) return;

    const username = this.auth.username();
    if (!username) return;

    const raw = this.form.getRawValue();
    const payload = {
      username,
      ...raw,
      expiration: raw.expirationMonth && raw.expirationYear
        ? `${raw.expirationMonth}/${raw.expirationYear.slice(-2)}`
        : '',
      customerId: raw.customerId || this.auth.customerId() || crypto.randomUUID()
    };

    this.basketService.checkout(payload).subscribe((response) => {
      if (response.isSuccess) {
        this.snackBar.open('Order placed successfully.', 'Close', { duration: 3000 });
        this.router.navigate(['/orders']);
      } else {
        this.snackBar.open('Checkout failed. Please try again.', 'Close', { duration: 3000 });
      }
    });
  }

  private toggleCardValidators(enabled: boolean) {
    const cardName = this.form.get('cardName');
    const cardNumber = this.form.get('cardNumber');
    const expirationMonth = this.form.get('expirationMonth');
    const expirationYear = this.form.get('expirationYear');
    const cvv = this.form.get('cvv');

    if (enabled) {
      cardName?.setValidators([Validators.required]);
      cardNumber?.setValidators([Validators.required, this.cardNumberValidator]);
      expirationMonth?.setValidators([Validators.required]);
      expirationYear?.setValidators([Validators.required]);
      cvv?.setValidators([Validators.required, Validators.pattern(/^\d{3}$/)]);
    } else {
      cardName?.clearValidators();
      cardNumber?.clearValidators();
      expirationMonth?.clearValidators();
      expirationYear?.clearValidators();
      cvv?.clearValidators();
      cardName?.setValue('');
      cardNumber?.setValue('');
      expirationMonth?.setValue('');
      expirationYear?.setValue('');
      cvv?.setValue('');
    }

    cardName?.updateValueAndValidity({ emitEvent: false });
    cardNumber?.updateValueAndValidity({ emitEvent: false });
    expirationMonth?.updateValueAndValidity({ emitEvent: false });
    expirationYear?.updateValueAndValidity({ emitEvent: false });
    cvv?.updateValueAndValidity({ emitEvent: false });
  }

  onCardNumberInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 16);
    const grouped = digits.replace(/(.{4})/g, '$1-').replace(/-$/, '');
    input.value = grouped;
    this.form.get('cardNumber')?.setValue(grouped, { emitEvent: false });
  }

  private cardNumberValidator(control: AbstractControl): ValidationErrors | null {
    const value = String(control.value || '');
    const digits = value.replace(/\D/g, '');
    return digits.length === 16 ? null : { cardNumber: true };
  }
}
