import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [ FormsModule, CommonModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent implements OnInit {
  showDeleteButton: boolean = true;
  documents: any[] = [];
  filteredDocuments: any[] = [];
  searchQuery: string = '';

  showViewer = false;
  selectedDoc: any = null;

  passwordPromptVisible = false;
  enteredPassword = '';
  passwordErrorMessage: string | null = null;

  blobUrl: SafeResourceUrl | null = null;

  passwordForDownload = false;
  errorMessage: string | null = null;
  cancelledPasswordPrompt: boolean = false;
  docToDelete: any = null;
  showConfirmationModal: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private sanitizer: DomSanitizer,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const apiUrl = `http://localhost:5138/api/document/my-documents`; // Only user-specific

    this.http.get<any[]>(apiUrl).subscribe({
      next: (data) => {
        this.documents = data
          .map(doc => ({
            ...doc,
            document_id: doc.documentId,
            uploadedAtFormatted: this.formatDate(doc.uploadedAt),
            fileSizeFormatted: this.formatFileSize(doc.fileSize),
            fileType: doc.fileType || 'Unknown'
          }))
          .sort((a, b) => new Date(b.uploadedAt).getTime() - new Date(a.uploadedAt).getTime());

        this.applyFilter();
      },
      error: (err) => {
        console.error('Failed to load documents:', err);
        this.showError('Failed to load documents.');
      }
    });
  }

  applyFilter() {
    const query = this.searchQuery?.toLowerCase() || '';
    this.filteredDocuments = this.documents.filter(doc =>
      doc.fileName.toLowerCase().includes(query)
    );
  }

  viewDocument(doc: any) {
  this.selectedDoc = doc;
  this.enteredPassword = '';
  this.passwordErrorMessage = null;
  this.cancelledPasswordPrompt = false;

  const extension = doc.fileName?.split('.').pop()?.toLowerCase();

  if (extension === 'pdf') {
    // Skip app password prompt – show encrypted PDF
    this.passwordPromptVisible = false;
    this.previewDocument(doc);
  } else {
    // Show app-level password prompt for .doc/.docx
    this.passwordForDownload = false;
    this.passwordPromptVisible = true;
  }
}


  confirmPasswordAndPreview() {
    if (!this.selectedDoc) return;

    this.cancelledPasswordPrompt = false;

    if (this.passwordForDownload) {
      this.downloadDocumentApi(this.selectedDoc, this.enteredPassword);
      this.passwordForDownload = false;
    } else {
      this.previewDocument(this.selectedDoc, this.enteredPassword);
    }
  }

  private previewDocument(doc: any, password?: string) {
    let headers = new HttpHeaders();

    if (password) {
      headers = headers.append('x-document-password', password);
    }

    const url = `http://localhost:5138/api/document/preview/${doc.document_id}`;

    this.http.get(url, { headers, responseType: 'blob' }).subscribe({
      next: (blob) => {
        this.passwordPromptVisible = false;
        this.passwordErrorMessage = null;
        this.cancelledPasswordPrompt = false;

        if (this.blobUrl) {
          URL.revokeObjectURL(this.blobUrl as string);
        }

        const blobUrl = URL.createObjectURL(blob);
        this.blobUrl = this.sanitizer.bypassSecurityTrustResourceUrl(blobUrl);
        this.showViewer = true;
      },
      error: (err) => {
        console.error('Failed to load document preview', err);

        if (err.status === 401 || err.status === 403) {
          this.showError('Incorrect password or insufficient permissions.');
        } else if (err.status === 400 && err.error) {
          this.showError(err.error);
        } else if (err.status === 404) {
          this.showError('Document not found or missing file.');
        } else {
          this.showError('Unable to preview document. Please check your permissions or password.');
        }
      }
    });
  }

  downloaddocument(doc: any) {
    this.selectedDoc = doc;
    this.enteredPassword = '';
    this.passwordErrorMessage = null;
    this.passwordPromptVisible = true;
    this.passwordForDownload = true;
    this.cancelledPasswordPrompt = false;
  }

  private downloadDocumentApi(doc: any, password: string) {
    const url = `http://localhost:5138/api/document/download/${doc.document_id}`;
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const body = { password: password || '' };

    this.http.post(url, body, { headers, responseType: 'blob' }).subscribe({
      next: (blob) => {
        const downloadUrl = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = downloadUrl;
        anchor.download = doc.fileName || 'downloaded_file';
        anchor.click();
        window.URL.revokeObjectURL(downloadUrl);

        this.passwordPromptVisible = false;
        this.enteredPassword = '';
        this.passwordErrorMessage = null;
        this.passwordForDownload = false;
        this.cancelledPasswordPrompt = false;
        this.selectedDoc = null;
      },
      error: (err) => {
        console.error('Download failed', err);
        if (err.status === 401 || err.status === 403) {
          this.showError('Incorrect password or insufficient permissions.');
        } else if (err.status === 404) {
          this.showError('Document not found or missing file.');
        } else if (err.status === 400 && err.error) {
          this.showError(err.error);
        } else {
          this.showError('Unable to download document. Please check your permissions or password.');
        }
      }
    });
  }

  promptDeleteDocument(doc: any): void {
    this.docToDelete = doc;
    this.showConfirmationModal = true;
  }

  proceedWithDeletion(): void {
    if (!this.docToDelete) return;

    const doc = this.docToDelete;
    const url = `http://localhost:5138/api/document/soft-delete/${doc.document_id}`;

    this.http.put(url, {}).subscribe({
      next: () => {
        this.documents = this.documents.filter(d => d.document_id !== doc.document_id);
        this.applyFilter();
        this.showError('Document deleted successfully.');
        this.resetConfirmationModal();
      },
      error: err => {
        console.error('Failed to soft delete document:', err);
        this.showError('Failed to delete the document. Please try again.');
        this.resetConfirmationModal();
      }
    });
  }

  cancelDelete(): void {
    this.resetConfirmationModal();
  }

  private resetConfirmationModal(): void {
    this.showConfirmationModal = false;
    this.docToDelete = null;
  }

  closePasswordPrompt() {
    this.passwordPromptVisible = false;
    this.enteredPassword = '';
    this.passwordForDownload = false;
    this.passwordErrorMessage = null;
    this.cancelledPasswordPrompt = true;
  }

  closeModal() {
    this.showViewer = false;
    this.selectedDoc = null;

    if (this.blobUrl) {
      URL.revokeObjectURL(this.blobUrl as string);
      this.blobUrl = null;
    }
  }

  private formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return isNaN(date.getTime()) ? 'N/A' : date.toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  private formatFileSize(sizeInBytes: number): string {
    if (!sizeInBytes || sizeInBytes <= 0) return '0 KB';
    if (sizeInBytes >= 1024 * 1024) {
      return (sizeInBytes / (1024 * 1024)).toFixed(2) + ' MB';
    } else {
      return (sizeInBytes / 1024).toFixed(2) + ' KB';
    }
  }

  showError(message: string, durationMs = 2500): void {
    if (this.passwordPromptVisible) {
      this.passwordErrorMessage = message;
    } else {
      this.errorMessage = message;
      setTimeout(() => this.errorMessage = null, durationMs);
    }
  }
  getFontAwesomeIcon(doc: any): string {
  const extension = doc.fileName?.split('.').pop()?.toLowerCase();
  switch (extension) {
    case 'pdf': return 'fa-file-pdf';
    case 'doc':
    case 'docx': return 'fa-file-word';
    default: return 'fa-file';
  }
}

getFileColorClass(doc: any): string {
  const extension = doc.fileName?.split('.').pop()?.toLowerCase();
  switch (extension) {
    case 'pdf': return 'pdf';
    case 'doc':
    case 'docx': return 'word';
    default: return 'default';
  }
}
}
