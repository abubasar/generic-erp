import { NgModule, ErrorHandler } from "@angular/core";
import { RouterModule } from "@angular/router";
import {
  BrowserModule,
  HAMMER_GESTURE_CONFIG,
} from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
// import { GestureConfig } from '@angular/material/core';
import {
  PerfectScrollbarModule,
  PERFECT_SCROLLBAR_CONFIG,
  PerfectScrollbarConfigInterface,
} from "./shared/components/perfect-scrollbar";

import { rootRouterConfig } from "./app.routing";
import { SharedModule } from "./shared/shared.module";
import { AppComponent } from "./app.component";

import {
  HttpClient,
  HttpClientModule,
  HTTP_INTERCEPTORS,
} from "@angular/common/http";
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { ErrorHandlerService } from "./shared/services/error-handler.service";
import { TokenInterceptor } from "./shared/interceptors/token.interceptor";
import { ToastrModule } from "ngx-toastr";
import { MAT_DATE_LOCALE } from "@angular/material/core";
import "moment/locale/en-gb";
import { JwtAuthService } from "./shared/services/auth/jwt-auth.service";
import { JwtModule, JWT_OPTIONS } from "@auth0/angular-jwt";
import { ValidationErrorInterceptor } from "./shared/interceptors/validation-error.interceptor";
import {NgxMatTimepickerModule} from "ngx-mat-timepicker";
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { NgxMatDatetimePickerModule, NgxMatNativeDateModule } from "@angular-material-components/datetime-picker";
import { TooManyRequestInterceptor } from "./shared/interceptors/too-many-request.interceptor";
import { Status412PreconditionFailed } from "./shared/interceptors/status-412-precondition-failed";
//.....
export function jwtOptionFactor(authService: JwtAuthService) {
  return {
    tokenGetter: () => {
      return authService.getAccessToken();
    },
    allowedDomains: ['localhost:4200'],
    disallowedRoutes: ['http://localhost:4200/auth/login'],
  };
}
// AoT requires an exported function for factories
export function HttpLoaderFactory(httpClient: HttpClient) {
  return new TranslateHttpLoader(httpClient);
}

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
  suppressScrollX: true,
};

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    SharedModule,
    HttpClientModule,
    PerfectScrollbarModule,
    NgxMatTimepickerModule.setLocale("en-GB"),
    NgxMatDatetimePickerModule,
    NgxMatNativeDateModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient],
      },
    }),

    RouterModule.forRoot(rootRouterConfig, {
      useHash: true,
      relativeLinkResolution: "legacy",
    }),
    ToastrModule.forRoot(),
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        useFactory: jwtOptionFactor,
        deps: [JwtAuthService],
      },
    }),
    ServiceWorkerModule.register("ngsw-worker.js", {
      enabled: environment.production,
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: "registerWhenStable:30000",
    }),
  ],
  declarations: [AppComponent],
  providers: [
    { provide: ErrorHandler, useClass: ErrorHandlerService },
    // { provide: HAMMER_GESTURE_CONFIG, useClass: GestureConfig },
    {
      provide: PERFECT_SCROLLBAR_CONFIG,
      useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG,
    },
    // REQUIRED IF YOU USE JWT AUTHENTICATION
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ValidationErrorInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TooManyRequestInterceptor,
      multi: true,
    },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: Status412PreconditionFailed,
      multi: true,
    },
    // Required to Mat Date Picker Date Format
    { provide: MAT_DATE_LOCALE, useValue: "en-GB" },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
