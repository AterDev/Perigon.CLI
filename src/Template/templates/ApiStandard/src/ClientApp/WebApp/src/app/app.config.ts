import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { CustomerHttpInterceptor } from './customer-http.interceptor';
import { TypedCellDefDirective } from './share/typed-cell-def.directive';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}


export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  ],
};


