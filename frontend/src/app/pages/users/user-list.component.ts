import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { alertError, alertSuccess, confirmRemoval } from '../../core/alerts';
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
  protected readonly currentUserId = readSession()?.id ?? null;

  ngOnInit(): void {
    this.title.setTitle('Usuarios · Padrón');
    this.load();
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
    void this.remove(user);
  }

  private async remove(user: Usuario): Promise<void> {
    const result = await confirmRemoval(user.username);
    if (!result.isConfirmed) {
      return;
    }

    this.userService.remove(user.id).subscribe({
      next: () => {
        this.users.update((users) => users.filter((item) => item.id !== user.id));
        void alertSuccess('Usuario eliminado.');
      },
      error: (error: unknown) => {
        void alertError(readApiError(error, 'No se pudo eliminar el usuario.'));
      },
    });
  }
}
