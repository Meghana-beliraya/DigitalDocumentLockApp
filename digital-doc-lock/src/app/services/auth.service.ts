import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

interface MyJwtPayload {
  userId: string;
  email: string;
  exp: number; // UNIX timestamp
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public baseUrl = 'http://localhost:5138/api';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  // Register User
  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Signup`, userData);
  }

  // Login User
  login(credentials: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Login/userLogin`, credentials);
  }

  // Save tokens
  saveToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('authToken', token);
    }
  }

  saveRefreshToken(refreshToken: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('refreshToken', refreshToken);
    }
  }

  // Get tokens
  getToken(): string | null {
    return isPlatformBrowser(this.platformId) ? localStorage.getItem('authToken') : null;
  }

  getRefreshToken(): string | null {
    return isPlatformBrowser(this.platformId) ? localStorage.getItem('refreshToken') : null;
  }

  // Remove tokens
  clearTokens(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
    }
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const decoded: MyJwtPayload = jwtDecode(token);
      const isExpired = decoded.exp < Math.floor(Date.now() / 1000);
      return !isExpired;
    } catch {
      return false;
    }
  }

  // Refresh token
  refreshToken(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return throwError(() => new Error('No refresh token'));

    return this.http.post(`${this.baseUrl}/Login/refresh`, { refreshToken }).pipe(
      switchMap((response: any) => {
        this.saveToken(response.token);
        this.saveRefreshToken(response.refreshToken);
        return of(response);
      }),
      catchError(err => {
        this.clearTokens();
        return throwError(() => err);
      })
    );
  }

  // Logout the user
  logout(): Observable<any> {
    const token = this.getToken();
    const headers = this.createAuthHeaders();

    return new Observable(observer => {
      this.http.post(`${this.baseUrl}/Login/logout`, {}, { headers }).subscribe({
        next: (res) => {
          this.clearTokens();
          observer.next(res);
          observer.complete();
        },
        error: (err) => {
          console.error('Logout failed:', err);
          this.clearTokens();
          observer.error(err);
        }
      });
    });
  }

  // Profile methods
  getProfile(): Observable<any> {
    return this.http.get(`${this.baseUrl}/user/profile`, { headers: this.createAuthHeaders() });
  }

  updateProfile(profileData: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/user/profile`, profileData, { headers: this.createAuthHeaders() });
  }

  // Save user info locally
  saveUserInfo(user: any): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('firstName', user.firstName);
      localStorage.setItem('email', user.email);
    }
  }

  getFirstName(): string | null {
    return isPlatformBrowser(this.platformId) ? localStorage.getItem('firstName') : null;
  }

  getCurrentUserEmail(): string | null {
    return isPlatformBrowser(this.platformId) ? localStorage.getItem('email') : null;
  }

  getUserIdFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      const userId = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
      return userId || null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  // Helper: Create Auth Headers
  private createAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  storeUserProfile(firstName: string, email: string): void {
  localStorage.setItem('firstName', firstName);
  localStorage.setItem('email', email);
}
}
