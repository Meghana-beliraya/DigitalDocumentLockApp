import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';  // Import RouterModule and Routes
import { BrowserModule } from '@angular/platform-browser';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome'; 
import { HttpClientModule } from '@angular/common/http';


// Import Standalone Components
import { AdminLayoutComponent } from './layout/layout.component'; // Standalone layout component
import { AdminDashboardComponent } from './dashboard/dashboard.component';
import { UsersComponent } from './users/users.component';
import { AdminDocumentsComponent } from './documents/documents.component';


// Define routes for the admin area
const adminRoutes: Routes = [
  { 
    path: '', 
    component: AdminLayoutComponent, // Use standalone layout component
    children: [
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'users', component: UsersComponent },
      { path: 'documents', component: AdminDocumentsComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(adminRoutes),  
    AdminLayoutComponent,  // Import Standalone Component here
    FontAwesomeModule,
    HttpClientModule,
  ],
  
})
export class AdminModule { }
