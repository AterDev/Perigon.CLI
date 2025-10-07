import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { CustomerHttpInterceptor } from './customer-http.interceptor';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideTranslateService({
      fallbackLang: 'zh',
      lang: 'zh',
      defaultLanguage: 'zh',
      extend: true,
      useDefaultLang: true,
    }),
    provideTranslateHttpLoader({
      prefix: './assets/i18n/',
      suffix: '.json',
    }),
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  ],
};




