import { Routes } from '@angular/router';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/auth/login/login.component';
import { SignupComponent } from './components/auth/signup/signup.component';
import { UploadComponent } from './components/upload/upload.component';
import { LayoutComponent } from './components/layout/layout.component';
import { UserComponent } from './components/user/user.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { DocumentsComponent } from './components/documents/documents.component';



// Imports of  Admin Components
import { AdminLayoutComponent } from './components/admin/layout/layout.component';
import { AdminDashboardComponent } from './components/admin/dashboard/dashboard.component';
import { UsersComponent } from './components/admin/users/users.component';
import { AdminDocumentsComponent } from './components/admin/documents/documents.component';
export const routes: Routes = [
  { path: '', component: HomeComponent },

  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'signup', component: SignupComponent },
    ]
  },
// Admin routes
{
  path: 'admin', // All admin-related routes are prefixed with 'admin'
  component: AdminLayoutComponent ,
  children: [
    { path: 'dashboard', component: AdminDashboardComponent },
    { path: 'users', component: UsersComponent },
    { path: 'documents', component: AdminDocumentsComponent },
  ]
},
  //  NESTED USER ROUTES
 {
  path: 'user',
  component: UserComponent,
  children: [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', component: DashboardComponent },
    { path: 'upload', component: UploadComponent },
    { path: 'documents/my', component: DocumentsComponent },
    { path: 'documents/all', component: DocumentsComponent }
  ]
}
,


  { path: '**', redirectTo: '', pathMatch: 'full' }
];


export const appRouter = provideRouter(routes);
