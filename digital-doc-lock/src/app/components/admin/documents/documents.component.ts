import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../services/admin.service';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-documents',
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss'],
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule]
})
export class AdminDocumentsComponent implements OnInit {
  documents: any[] = [];

  deleteTarget: any = null;
  showDeleteConfirm = false;
  deleteMessage: string = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.fetchDocuments();
  }

  fetchDocuments(): void {
    this.adminService.getAllDocuments().subscribe({
      next: (data) => {
        this.documents = data.map(doc => ({
          name: doc.fileName || 'Unnamed',
          owner: doc.fullName || 'Unknown',
          uploadDate: new Date(doc.uploadedAt).toLocaleDateString('en-GB'),
          documentId: doc.documentId || doc.DocumentId || null,
          fileType: (doc.fileName || '').split('.').pop()?.toLowerCase() || ''
        }));
      },
      error: (err) => {
        console.error('Error fetching documents:', err);
      }
    });
  }

  viewDocument(doc: any): void {
    const docId = doc?.documentId;
    if (!docId) return;

    this.adminService.previewDocument(docId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
      error: () => {
        alert('Preview failed');
      }
    });
  }

  confirmDelete(doc: any): void {
    this.deleteTarget = doc;
    this.deleteMessage = `Are you sure you want to delete the document "${doc.name}"?`;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.deleteTarget = null;
  }

  proceedDelete(): void {
    const docId = this.deleteTarget?.documentId;
    if (!docId) {
      alert('Invalid document ID');
      this.cancelDelete();
      return;
    }

    this.adminService.softDeleteDocument(docId).subscribe({
      next: () => {
        this.fetchDocuments(); // fresh list
        this.cancelDelete();
      },
      error: (err) => {
        console.error('Delete error:', err);
        alert('Failed to delete document.');
        this.cancelDelete();
      }
    });
  }
}
