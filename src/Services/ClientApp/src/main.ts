import { enableProdMode } from '@angular/core';
import { environment } from './environments/environment';
import { bootstrapApplication } from '@angular/platform-browser';
import { MarkedRenderer } from 'ngx-markdown';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

const renderer = new MarkedRenderer();

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.log(err));
