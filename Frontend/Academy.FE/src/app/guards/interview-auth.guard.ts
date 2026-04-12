import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { VideoRecorderService } from '@services/video-recorder.service';
import { AuthenticationService } from '@services/authentication.service'; 

export const interviewAuthGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state) => {
  const videoRecordService = inject(VideoRecorderService);
  const router = inject(Router);
  const authService = inject(AuthenticationService); 

  if (!authService.isLoggedIn) {
    const attemptedUrl = state.url;  
    localStorage.setItem('attemptedUrl', attemptedUrl); 
    router.navigate(['/login']);
    return of(false);
  }
  const interviewId = route.paramMap.get('id');

  if (!interviewId) {
    router.navigate(['/invalid']);
    return of(false);  
  }

  return videoRecordService.validateInterviewId(interviewId).pipe(
    map((isValid: boolean) => {
   
      if (isValid) {
        return true;
      } else {
        router.navigate(['/invalid']);
        return false; 
      }
    }),
    catchError(() => {

      router.navigate(['/invalid']);
      return of(false); 
    })
  );
};
