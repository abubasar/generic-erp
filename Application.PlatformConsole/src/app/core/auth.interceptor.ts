import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.token;
  const authed = token && req.url.startsWith('/api/platform') && !req.url.endsWith('/auth/login')
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authed).pipe(
    tap((event: any) => {
      // The API replies 200 with { succeeded:false, statusCode:401 } on auth failure.
      if (event?.body && event.body.succeeded === false && event.body.statusCode === 401) {
        auth.logout();
        router.navigate(['/login']);
      }
    }),
  );
};
