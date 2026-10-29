import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { readApiError } from '../../core/api-error';
import { AuthService } from '../../services/auth.service';

const REMEMBER_KEY = 'padron.rememberUsername';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);
  protected readonly loginFailed = signal(false);
  protected readonly loginMessage = signal('Usuario o contraseña incorrectos.');

  protected readonly form = this.formBuilder.nonNullable.group({
    username: ['', [Validators.required, Validators.maxLength(50)]],
    password: ['', [Validators.required]],
    remember: [false],
  });

  constructor() {
    inject(Title).setTitle('Ingresar · Sistema de gestión de usuarios');
    const saved = localStorage.getItem(REMEMBER_KEY);
    if (saved) {
      this.form.controls.username.setValue(saved);
      this.form.controls.remember.setValue(true);
    }
    this.form.controls.username.valueChanges.subscribe(() => this.loginFailed.set(false));
    this.form.controls.password.valueChanges.subscribe(() => this.loginFailed.set(false));
  }

  protected showError(name: 'username' | 'password'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected showAlert(name: 'username' | 'password'): boolean {
    return this.showError(name) || this.loginFailed();
  }

  protected alertText(name: 'username' | 'password'): string {
    if (this.showError(name)) {
      return name === 'username' ? 'El usuario es obligatorio.' : 'La contraseña es obligatoria.';
    }
    return this.loginMessage();
  }

  protected banner(): string | null {
    const messages: string[] = [];
    if (this.showError('username')) {
      messages.push(this.alertText('username'));
    }
    if (this.showError('password')) {
      messages.push(this.alertText('password'));
    }
    if (messages.length > 0) {
      return messages.join(' ');
    }
    return this.loginFailed() ? this.loginMessage() : null;
  }

  protected submit(): void {
    this.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    const { username, password, remember } = this.form.getRawValue();
    const name = username.trim();
    this.auth.login(name, password).subscribe({
      next: () => {
        if (remember) {
          localStorage.setItem(REMEMBER_KEY, name);
        } else {
          localStorage.removeItem(REMEMBER_KEY);
        }
        this.submitting.set(false);
        void this.router.navigate(['/usuarios']);
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.loginMessage.set(readApiError(error, 'Usuario o contraseña incorrectos.'));
        this.loginFailed.set(true);
      },
    });
  }
}
