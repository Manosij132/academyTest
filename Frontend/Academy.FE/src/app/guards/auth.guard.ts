import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authToken = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null; // Replace 'authToken' with your token key

  // Check if the token is undefined, null, or empty
  if (!authToken || authToken.trim() === '') {
    router.navigate(['/login']); // Redirect to login page
    return false; // Prevent access to the route
  }
  // router.navigate(['/login']); 
  return true; // Allow access to the route
};
