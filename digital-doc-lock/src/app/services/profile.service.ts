import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AuthService } from './auth.service';

export interface Profile {
  firstName: string;
  lastName: string;
  email: string;
  profileImageUrl?: string; // updated to be consistent with backend
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  oldPassword: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly apiBaseUrl = 'http://localhost:5138/api/profile';
  private readonly backendUrl = 'http://localhost:5138';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}


  getProfile(): Observable<Profile> {
    return this.http.get<Profile>(`${this.apiBaseUrl}/me`).pipe(
      map(profile => ({
        ...profile,
        profileImageUrl: profile.profileImageUrl
          ? this.getFormattedImageUrl(profile.profileImageUrl)
          : 'assets/profile.png'
      }))
    );
  }

  /**
   * Format profile image URL with cache busting timestamp
   */
  private getFormattedImageUrl(relativePath: string): string {
    return `${this.backendUrl}${relativePath}?v=${new Date().getTime()}`;
  }

  updateProfile(updateData: UpdateProfileRequest): Observable<any> {
    return this.http.put(`${this.apiBaseUrl}/update`, updateData);
  }


  uploadProfileImage(file: File): Observable<{ profileImageUrl: string }> {
  const formData = new FormData();
  formData.append('image', file);

  return this.http.post<{ imageUrl: string }>(`${this.apiBaseUrl}/upload-image`, formData).pipe(
    map(res => ({
      profileImageUrl: this.getFormattedImageUrl(res.imageUrl)
    }))
  );
}

}
