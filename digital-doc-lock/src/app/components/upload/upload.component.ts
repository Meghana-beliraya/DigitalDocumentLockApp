import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  selectedFile: File | null = null;
  password: string = '';
  allowAdminView: boolean = true;
  uploadMessage: string = '';
  isUploading: boolean = false;

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (!this.isAllowedFileType(file)) {
        this.uploadMessage = '❌ File type not accepted. Only doc, docx and pdf is accepted.';
        this.selectedFile = null;
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        this.uploadMessage = '❌ File size exceeds 5MB. Please select a smaller file.';
        this.selectedFile = null;
        return;
      }

      this.selectedFile = file;
      this.uploadMessage = '';
      console.log("File selected:", this.selectedFile);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onDropFile(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer?.files.length) {
      const file = event.dataTransfer.files[0];

      if (!this.isAllowedFileType(file)) {
        this.uploadMessage = '❌ File type not accepted. Only doc, docx and pdf is accepted.';
        this.selectedFile = null;
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        this.uploadMessage = '❌ File size exceeds 5MB. Please select a smaller file.';
        this.selectedFile = null;
        return;
      }

      this.selectedFile = file;
      this.uploadMessage = '';
      console.log("File dropped:", this.selectedFile);
    }
  }

  onUpload() {
    if (!this.selectedFile || !this.password) {
      this.uploadMessage = '⚠️ Please fill all required fields.';
      return;
    }

    const token = localStorage.getItem('authToken')?.trim();
    console.log("Token from localStorage:", token);
    if (!token) {
      this.uploadMessage = '❌ Unauthorized. Please login again.';
      return;
    }

    const formData = new FormData();
    formData.append('File', this.selectedFile);
    formData.append('Password', this.password);
    formData.append('AllowAdminView', this.allowAdminView.toString());

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.isUploading = true;

    this.http.post('http://localhost:5138/api/document/upload', formData, { headers }).subscribe({
      next: (response: any) => {
        console.log("Upload response:", response);
        this.uploadMessage = '✅ File uploaded successfully!';
        this.resetForm();
      },
      error: (err) => {
        console.error("Upload error status:", err.status);
        console.error("Upload error response:", err.error);
        console.error("Upload error headers:", err.headers);
        if (err.status === 401) {
          this.uploadMessage = '❌ Unauthorized. Please login again.';
        } else {
          this.uploadMessage = '❌ Upload failed. Check server or try again.';
        }
      },
      complete: () => {
        this.isUploading = false;
        setTimeout(() => this.uploadMessage = '', 3000);
      }
    });
  }

  private resetForm() {
    this.selectedFile = null;
    this.password = '';
    this.allowAdminView = true;
  }

  private isAllowedFileType(file: File): boolean {
    const allowedTypes = [
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
    ];
    return allowedTypes.includes(file.type);
  }
}
