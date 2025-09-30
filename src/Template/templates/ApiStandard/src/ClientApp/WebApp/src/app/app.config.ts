import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { CustomerHttpInterceptor } from './customer-http.interceptor';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}


export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  ],
};


