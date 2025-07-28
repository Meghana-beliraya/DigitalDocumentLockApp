import { bootstrapApplication } from '@angular/platform-browser'; // application routes
import { provideRouter } from '@angular/router'; 
import { AppComponent } from './app/app.component'; // root component 
import { routes } from './app/app.routes';
import { importProvidersFrom } from '@angular/core'; //use NgModules
import { FormsModule } from '@angular/forms'; 
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http'; // configuring HttpClient services// Added withInterceptors
import { BrowserModule } from '@angular/platform-browser';
import { AuthInterceptor } from './app/interceptors/auth.interceptor';


bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    importProvidersFrom(BrowserModule, FormsModule),
    provideHttpClient(
  withFetch(),
  withInterceptors([AuthInterceptor])
),
  ]
}).catch(err => console.error(err));
