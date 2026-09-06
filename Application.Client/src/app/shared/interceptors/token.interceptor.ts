import { Injectable } from "@angular/core";
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest
} from "@angular/common/http";
import { Observable, catchError, switchMap, throwError } from "rxjs";
import { JwtAuthService } from "../services/auth/jwt-auth.service";
import { JwtHelperService } from "@auth0/angular-jwt";
import { Router } from "@angular/router";
import { TokenModel } from "../models/token.model";
import { UserProfile } from "../models/user-profile-model";
import { GeneralResponse } from "../models/wrappers/generalResponse.model";

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor(
    //private jwtHelper: JwtHelperService,
    private authService: JwtAuthService,
    private router: Router) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (req.url.indexOf('login') > -1 || req.url.indexOf('refresh-token') > -1) {
      return next.handle(req);
    }
    const localStorageTokens = localStorage.getItem('tokens');
    if(localStorageTokens=='undefined')  return next.handle(req);
    var token: TokenModel;
    if (localStorageTokens) {
      token = JSON.parse(localStorageTokens) as TokenModel;
      var jwtHelper: JwtHelperService=new JwtHelperService();
      var isTokenExpired = jwtHelper.isTokenExpired(token?.accessToken);
      //
      if (!isTokenExpired) {
        const request = req.clone({
          headers: req.headers.set(
            'Authorization',
            `Bearer ${token?.accessToken}`
          ),
        });
        return next.handle(request);
      } else {
      return this.authService.refreshToken(token).pipe(
        switchMap((res: GeneralResponse<TokenModel>) => {
          if(res.succeeded==true){
            // Store the new tokens in local storage
            localStorage.setItem("tokens", JSON.stringify(res?.data));
            // Clone the request and set the new access token in the Authorization header
            const transformedReq = req.clone({
              headers: req.headers.set(
                "Authorization",
                `Bearer ${res?.data?.accessToken}`
              ),
            });
            // Continue handling the request
            return next.handle(transformedReq);
          }else{
            this.router.navigate(["/sessions/signin"]);
            return throwError(() => 'error');
          }
         
        }),
        catchError((error) => {
          this.router.navigate(["/sessions/signin"]);
          return throwError(() => error);
        })
      );

      }
    }
    this.router.navigate(['/']);
    return throwError(() => 'Invalid call');
  }
}