import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './guards/auth.guard';
import { ShellComponent } from './layout/shell.component';
import { LoginComponent } from './pages/login/login.component';
import { UserFormComponent } from './pages/users/user-form.component';
import { UserListComponent } from './pages/users/user-list.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard],
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'usuarios' },
      { path: 'usuarios', component: UserListComponent },
      { path: 'usuarios/nuevo', component: UserFormComponent },
      { path: 'usuarios/:id/editar', component: UserFormComponent },
      { path: '**', redirectTo: 'usuarios' },
    ],
  },
];
