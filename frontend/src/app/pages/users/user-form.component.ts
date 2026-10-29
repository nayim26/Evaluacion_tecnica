import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { readApiError } from '../../core/api-error';
import { readSession } from '../../core/session';
import { UserService } from '../../services/user.service';

const passwordsMatch: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = String(control.get('password')?.value ?? '');
  const confirm = String(control.get('confirmPassword')?.value ?? '');
  if (!password && !confirm) {
    return null;
  }
  return password === confirm ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './user-form.component.html',
})
export class UserFormComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly mode = signal<'create' | 'edit'>('create');
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly submitted = signal(false);
  protected readonly showForm = signal(true);
  protected readonly pageError = signal('');
  protected readonly createdAt = signal<string | null>(null);
  private userId: number | null = null;
  private originalUsername = '';
  private loadGeneration = 0;

  protected readonly form = this.formBuilder.nonNullable.group(
    {
      username: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
          Validators.pattern(/^[a-zA-Z0-9._-]+$/),
        ],
      ],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      password: [''],
      confirmPassword: [''],
      estado: ['activo'],
    },
    { validators: [passwordsMatch] },
  );

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.setup(params.get('id'));
    });
  }

  protected showError(name: 'username' | 'email' | 'password' | 'confirmPassword'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected errorMessage(name: 'username' | 'email' | 'password' | 'confirmPassword'): string {
    const errors = this.form.controls[name].errors;
    if (!errors) {
      return '';
    }
    if (errors['required']) {
      return 'Este campo es obligatorio.';
    }
    if (errors['email']) {
      return 'Ingrese un correo válido.';
    }
    if (errors['minlength']) {
      const requiredLength = (errors['minlength'] as { requiredLength: number }).requiredLength;
      return `Mínimo ${requiredLength} caracteres.`;
    }
    if (errors['maxlength']) {
      const requiredLength = (errors['maxlength'] as { requiredLength: number }).requiredLength;
      return `Máximo ${requiredLength} caracteres.`;
    }
    if (errors['pattern']) {
      return 'Use letras, números, punto, guion o guion bajo.';
    }
    return 'Revise este campo.';
  }

  protected submit(): void {
    this.submitted.set(true);
    this.pageError.set('');
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) {
      return;
    }

    const value = this.form.getRawValue();
    const sessionId = readSession()?.id ?? null;
    if (this.userId !== null && this.userId === sessionId && value.estado === 'inactivo') {
      this.pageError.set('No puede desactivar el usuario con el que inició sesión.');
      return;
    }

    const estado = value.estado === 'activo';
    this.saving.set(true);

    if (this.mode() === 'create') {
      this.userService
        .create({
          username: value.username.trim(),
          email: value.email.trim(),
          password: value.password,
          estado,
        })
        .subscribe({
          next: () => this.finish('Usuario creado.'),
          error: (error: unknown) => this.fail(error, 'No se pudo crear el usuario.'),
        });
      return;
    }

    if (this.userId === null) {
      this.saving.set(false);
      return;
    }

    this.userService
      .update(this.userId, {
        username: value.username.trim(),
        email: value.email.trim(),
        estado,
        ...(value.password ? { password: value.password } : {}),
      })
      .subscribe({
        next: () => {
          const renamed = this.userId === sessionId && value.username.trim() !== this.originalUsername;
          this.finish(
            renamed
              ? 'Usuario actualizado. Vuelva a iniciar sesión para ver su nuevo nombre.'
              : 'Usuario actualizado.',
          );
        },
        error: (error: unknown) => this.fail(error, 'No se pudo actualizar el usuario.'),
      });
  }

  private setup(idParam: string | null): void {
    this.submitted.set(false);
    this.pageError.set('');
    this.saving.set(false);
    this.createdAt.set(null);
    this.originalUsername = '';
    this.form.reset({
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      estado: 'activo',
    });

    if (idParam === null) {
      this.mode.set('create');
      this.userId = null;
      this.showForm.set(true);
      this.loading.set(false);
      this.form.controls.password.setValidators([
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(100),
      ]);
      this.form.controls.password.updateValueAndValidity();
      this.title.setTitle('Nuevo usuario · Padrón');
      return;
    }

    const id = Number(idParam);
    if (!Number.isInteger(id) || id <= 0) {
      this.mode.set('edit');
      this.userId = null;
      this.showForm.set(false);
      this.loading.set(false);
      this.pageError.set('El identificador del usuario no es válido.');
      return;
    }

    this.mode.set('edit');
    this.userId = id;
    this.showForm.set(true);
    this.loading.set(true);
    this.form.controls.password.setValidators([Validators.minLength(6), Validators.maxLength(100)]);
    this.form.controls.password.updateValueAndValidity();
    this.title.setTitle('Editar usuario · Padrón');

    const generation = ++this.loadGeneration;
    this.userService.getById(id).subscribe({
      next: (user) => {
        if (generation !== this.loadGeneration) {
          return;
        }
        this.createdAt.set(user.fechaCreacion);
        this.originalUsername = user.username;
        this.form.patchValue({
          username: user.username,
          email: user.email,
          estado: user.estado ? 'activo' : 'inactivo',
          password: '',
          confirmPassword: '',
        });
        this.loading.set(false);
      },
      error: (error: unknown) => {
        if (generation !== this.loadGeneration) {
          return;
        }
        this.loading.set(false);
        this.showForm.set(false);
        this.pageError.set(readApiError(error, 'No se pudo cargar el usuario.'));
      },
    });
  }

  private finish(notice: string): void {
    this.saving.set(false);
    void this.router.navigate(['/usuarios'], { state: { notice } });
  }

  private fail(error: unknown, fallback: string): void {
    this.saving.set(false);
    this.pageError.set(readApiError(error, fallback));
  }
}
