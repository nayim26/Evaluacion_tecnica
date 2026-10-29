import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { clearSession, isSessionActive } from '../core/session';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  if (isSessionActive()) {
    return true;
  }

  clearSession();
  return router.createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);
  if (!isSessionActive()) {
    return true;
  }

  return router.createUrlTree(['/usuarios']);
};
