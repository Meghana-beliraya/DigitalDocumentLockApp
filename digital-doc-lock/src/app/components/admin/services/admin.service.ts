import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'; //http req & custom header(Content-Type)
import { Observable } from 'rxjs';

// Interfaces
interface ActivityLogDto {
  timestamp: string;
  message: string;
  userId: number;
  firstName: string;
}

interface DashboardStatsDto {
  totalUsers: number;
  totalDocuments: number;
  documentsUploadedToday: number;
  activityLogs: ActivityLogDto[];
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private baseUrl = 'http://localhost:5138/api';
  private dashboardApiUrl = `${this.baseUrl}/dashboard/admin/data`;
  private documentApiBaseUrl = `${this.baseUrl}/document`;
  private adminDocumentsApiUrl = `${this.baseUrl}/admin/documents`;

  constructor(private http: HttpClient) {}

  // Get dashboard data
  getAdminDashboardData(): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>(this.dashboardApiUrl);
  }

  // Get all documents for admin
  getAllDocuments(): Observable<any[]> {
    return this.http.get<any[]>(this.adminDocumentsApiUrl);
  }

  // Preview document (supports optional password for Word docs)
previewDocument(documentId: number): Observable<Blob> {
  return this.http.get(`${this.baseUrl}/document/preview/${documentId}`, {
    responseType: 'blob'
  });
}

  // Soft delete a document
  softDeleteDocument(documentId: number): Observable<any> {
    return this.http.put(`${this.documentApiBaseUrl}/soft-delete/${documentId}`, null);
  }
}
