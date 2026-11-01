
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route) => {

  const auth = inject(Auth);
  const router = inject(Router);

  const allowedRoles = route.data['roles'] as string[];
  const userRole = auth.getUserRole();

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
