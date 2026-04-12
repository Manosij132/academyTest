import {
  GoogleLoginProvider, SocialAuthService, SocialAuthServiceConfig,
  SocialLoginModule,
} from "@abacritt/angularx-social-login";
import { provideHttpClient, withFetch, withInterceptors } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from "@angular/core";
import { provideAnimations } from "@angular/platform-browser/animations";
import { provideRouter } from "@angular/router";
import { provideToastr } from "ngx-toastr";
import { environment } from "@environments/environment";
import { routes } from "./app.routes";
import { AuthInterceptor } from "@interceptors/Auth.interceptor";
import { AuthenticationService } from "@services/authentication.service";
import { MatNativeDateModule } from '@angular/material/core';
import { ApiResponseInterceptor } from "./interceptors/api-response.interceptor";
import { provideQuillConfig } from "ngx-quill";
import { loaderInterceptor } from "./interceptors/LoaderInterceptor";

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    SocialLoginModule,
    SocialAuthService,
    {
      provide: "SocialAuthServiceConfig",
      useValue: {
        autoLogin: true,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(
              environment.GoogleProviderClientId
            ),
          },
        ],
        onError: (err) => {
          console.error(err);
        },
      } as SocialAuthServiceConfig,
    },
    AuthenticationService,
    provideHttpClient(
      withFetch(),
      withInterceptors([AuthInterceptor, loaderInterceptor, ApiResponseInterceptor])
    ),
    provideAnimations(),
    provideToastr({
      closeButton: true,
      timeOut: 5000,
      positionClass: "toast-top-right",
      preventDuplicates: true,
    }),
    importProvidersFrom(MatNativeDateModule),
    provideQuillConfig({})
  ]
};
