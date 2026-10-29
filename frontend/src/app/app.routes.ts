import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/shell.component').then((m) => m.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'usuarios' },
      {
        path: 'usuarios',
        loadComponent: () =>
          import('./pages/users/user-list.component').then((m) => m.UserListComponent),
      },
      {
        path: 'usuarios/nuevo',
        loadComponent: () =>
          import('./pages/users/user-form.component').then((m) => m.UserFormComponent),
      },
      {
        path: 'usuarios/:id/editar',
        loadComponent: () =>
          import('./pages/users/user-form.component').then((m) => m.UserFormComponent),
      },
      { path: '**', redirectTo: 'usuarios' },
    ],
  },
];
