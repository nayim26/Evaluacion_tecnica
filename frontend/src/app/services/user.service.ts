import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Usuario, UsuarioCreate, UsuarioUpdate } from '../core/models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>('/api/users');
  }

  getById(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`/api/users/${id}`);
  }

  create(payload: UsuarioCreate): Observable<Usuario> {
    return this.http.post<Usuario>('/api/users', payload);
  }

  update(id: number, payload: UsuarioUpdate): Observable<Usuario> {
    return this.http.put<Usuario>(`/api/users/${id}`, payload);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`/api/users/${id}`);
  }
}
