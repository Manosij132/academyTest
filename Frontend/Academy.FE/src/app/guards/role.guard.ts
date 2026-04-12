import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '@services/authentication.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthenticationService);

  const allowedRoles: string[] | undefined =
    route.data?.['roles'];

  // If no roles defined → allow access
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const user = authService.userDetails;
  const userRoles = user?.roles ?? [];

  const hasAccess = userRoles.some(r =>
    r.roleName && allowedRoles.includes(r.roleName)
  );

  if (!hasAccess) {
    router.navigate(['/unauthorized']);
    return false;
  }

  return true;
};