import { CommonModule } from "@angular/common";
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { provideNativeDateAdapter } from "@angular/material/core";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatTooltipModule } from "@angular/material/tooltip";
import { provideRouter } from "@angular/router";
import { CLIPBOARD_OPTIONS, ClipboardButtonComponent, MarkdownModule, MARKED_OPTIONS, MarkedOptions, MarkedRenderer } from "ngx-markdown";
import { CustomerHttpInterceptor } from "./share/customer-http.interceptor";
import { routes } from './app.routes';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from "@angular/material/form-field";
import { MAT_SNACK_BAR_DEFAULT_OPTIONS } from "@angular/material/snack-bar";
import { MonacoEditorModule } from "ngx-monaco-editor-v2";


export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(MarkdownModule.forRoot({
      markedOptions: {
        provide: MARKED_OPTIONS,
        useFactory: markedOptionsFactory
      },
      clipboardOptions: {
        provide: CLIPBOARD_OPTIONS,
        useValue: {
          buttonComponent: ClipboardButtonComponent,
        },
      },
    }), MonacoEditorModule.forRoot()),

    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptorsFromDi()),
    provideNativeDateAdapter(),
    { provide: MAT_SNACK_BAR_DEFAULT_OPTIONS, useValue: { duration: 2500 } },
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } },
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  ],
};



export function markedOptionsFactory(): MarkedOptions {
  const renderer = new MarkedRenderer();
  renderer.blockquote = (text: string) => {
    return '<blockquote class="blockquote"><p>' + text + '</p></blockquote>';
  };
  // renderer.code = (code: string) => {
  //   return '<code class="inline-code">' + code + '</code>'
  // }
  return {
    renderer: renderer,
    gfm: true,
    breaks: false,
    pedantic: false
  };
}

export const baseMatModules = [
  CommonModule, MatIconModule, MatTooltipModule, MatButtonModule, MatProgressSpinnerModule, MatToolbarModule
];

