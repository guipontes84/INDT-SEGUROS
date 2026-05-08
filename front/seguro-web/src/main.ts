import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { LOCALE_ID } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';

registerLocaleData(localePt);

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    { provide: LOCALE_ID, useValue: 'pt-BR' }
  ]
}).catch((error) => console.error(error));
