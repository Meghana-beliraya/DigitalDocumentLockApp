import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import {
  HttpRequest,
  HttpHandlerFn,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { Router } from '@angular/router';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const AuthInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getToken();

  let authReq = req;
  if (token) {
    authReq = addTokenHeader(req, token);
  }

  return next(authReq).pipe(
    catchError((error) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !req.url.includes('/Login/refresh') // Prevent infinite refresh loop
      ) {
        return handle401Error(authService, req, next, router);
      }
      return throwError(() => error);
    })
  );
};

function addTokenHeader(req: HttpRequest<any>, token: string): HttpRequest<any> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function handle401Error(
  authService: AuthService,
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  router: Router
): Observable<HttpEvent<any>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap((res) => {
        isRefreshing = false;

        const newToken = res.token;
        const newRefreshToken = res.refreshToken;

        // Save tokens
        authService.saveToken(newToken);
        authService.saveRefreshToken(newRefreshToken);

        // Notify waiting requests
        refreshTokenSubject.next(newToken);

        return next(addTokenHeader(request, newToken));
      }),
      catchError((err) => {
        isRefreshing = false;

        // Clear tokens and redirect to login
        authService.clearTokens();
        router.navigate(['/login']); // Optional: change path as needed

        return throwError(() => err);
      })
    );
  } else {
    // Wait for the token to be refreshed
    return refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => next(addTokenHeader(request, token!)))
    );
  }
}
