import { DatePipe } from '@angular/common';
import { Component, HostListener, OnInit, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { readApiError } from '../../core/api-error';
import { Usuario } from '../../core/models';
import { readSession } from '../../core/session';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './user-list.component.html',
})
export class UserListComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly title = inject(Title);

  protected readonly users = signal<Usuario[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly notice = signal('');
  protected readonly pending = signal<Usuario | null>(null);
  protected readonly confirming = signal(false);
  protected readonly deleteError = signal('');
  protected readonly currentUserId = readSession()?.id ?? null;

  ngOnInit(): void {
    this.title.setTitle('Usuarios · Padrón');
    const state = history.state as { notice?: unknown } | null;
    this.notice.set(typeof state?.notice === 'string' ? state.notice : '');
    this.load();
  }

  @HostListener('document:keydown.escape')
  protected closeOnEscape(): void {
    if (!this.confirming()) {
      this.pending.set(null);
    }
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set('');
    this.userService.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.error.set(readApiError(error, 'No se pudo cargar el listado.'));
      },
    });
  }

  protected askDelete(user: Usuario): void {
    if (user.id === this.currentUserId) {
      return;
    }
    this.deleteError.set('');
    this.pending.set(user);
  }

  protected confirmDelete(): void {
    const target = this.pending();
    if (!target || this.confirming()) {
      return;
    }

    this.confirming.set(true);
    this.deleteError.set('');
    this.userService.remove(target.id).subscribe({
      next: () => {
        this.users.update((users) => users.filter((user) => user.id !== target.id));
        this.notice.set('Usuario eliminado.');
        this.pending.set(null);
        this.confirming.set(false);
      },
      error: (error: unknown) => {
        this.confirming.set(false);
        this.deleteError.set(readApiError(error, 'No se pudo eliminar el usuario.'));
      },
    });
  }
}
