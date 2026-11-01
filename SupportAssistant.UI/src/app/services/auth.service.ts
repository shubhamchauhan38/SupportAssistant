
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { LoginRequest } from '../models/login-request';
import { AuthResponse } from '../models/auth-response';

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private apiUrl = 'https://localhost:7040/api/auth';

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/login`,
      request
    ).pipe(
      tap(response => {
        sessionStorage.setItem('authToken', response.token);
        sessionStorage.setItem('userEmail', response.email);
        sessionStorage.setItem('userRole', response.role);
      })
    );
  }

  getToken(): string | null {
    return sessionStorage.getItem('authToken');
  }

  getUserRole(): string | null {
    return sessionStorage.getItem('userRole');
  }

  getUserEmail(): string | null {
    return sessionStorage.getItem('userEmail');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    sessionStorage.clear();
  }
}
