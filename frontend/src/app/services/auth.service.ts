import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { apiUrl } from '../core/api';
import { LoginResponse } from '../core/models';
import { clearSession, writeSession } from '../core/session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(apiUrl('/api/auth/login'), { username, password })
      .pipe(tap((session) => writeSession(session)));
  }

  logout(): void {
    clearSession();
  }
}
