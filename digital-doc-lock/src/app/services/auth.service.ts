import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

interface MyJwtPayload {
  userId: string;
  email: string;
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

  // Check if user is authenticated
  isAuthenticated(): boolean {
    return isPlatformBrowser(this.platformId) && !!localStorage.getItem('authToken');
  }

  // Save the JWT Token
  saveToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('authToken', token);
    }
  }

  // Get the JWT Token
  getToken(): string | null {
    return isPlatformBrowser(this.platformId) ? localStorage.getItem('authToken') : null;
  }

  // Logout the user
  logout(): Observable<any> {
    const token = this.getToken();

    if (!token) {
      return new Observable(observer => {
        if (isPlatformBrowser(this.platformId)) {
          localStorage.removeItem('authToken');
        }
        observer.complete();
      });
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return new Observable(observer => {
      this.http.post(`${this.baseUrl}/Login/logout`, {}, { headers }).subscribe({
        next: (res) => {
          if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem('authToken');
          }
          observer.next(res);
          observer.complete();
        },
        error: (err) => {
          console.error('Logout API failed:', err);
          if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem('authToken');
          }
          observer.error(err);
        }
      });
    });
  }

  // Get user profile
  getProfile() {
    const token = this.getToken();
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get(`${this.baseUrl}/user/profile`, { headers });
  }

  // Update user profile
  updateProfile(profileData: any) {
    const token = this.getToken();
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.put(`${this.baseUrl}/user/profile`, profileData, { headers });
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
      console.log('Decoded token:', decoded);

      const userId = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
      return userId || null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  storeUserProfile(firstName: string, email: string): void {
  localStorage.setItem('firstName', firstName);
  localStorage.setItem('email', email);
}

}
