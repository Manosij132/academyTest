import { inject } from '@angular/core';
import {
  HttpInterceptorFn
} from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { LoaderService } from '../services/loader.service';
import { SKIP_LOADER } from '../context/skip-loader.context';

export const loaderInterceptor: HttpInterceptorFn = (req, next) => {
  const loaderService = inject(LoaderService);

  if (req.context.get(SKIP_LOADER)) {
    return next(req);
  }

  loaderService.start();

  return next(req).pipe(
    finalize(() => {
      loaderService.stop();
    })
  );
};
