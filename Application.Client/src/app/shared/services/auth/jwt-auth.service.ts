import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { JwtHelperService } from "@auth0/angular-jwt";
import { TokenModel } from "app/shared/models/token.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { environment } from "environments/environment";
import { BehaviorSubject, of } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { User } from "../../models/user.model";
import { LocalStoreService } from "../local-store.service";

// ================= only for demo purpose ===========

const DEMO_USER: User = {
  id: "5b700c45639d2c0c54b354ba",
  displayName: "Demo",
  role: "SA",
};
// ================= you will get those data from server =======

@Injectable({
  providedIn: "root",
})
export class JwtAuthService {
  userProfile = new BehaviorSubject<UserProfile | null>(null);
  jwtService: JwtHelperService = new JwtHelperService();
  token;
  isAuthenticated: Boolean;
  user: User = {};
  user$ = new BehaviorSubject<User>(this.user);
  signingIn: Boolean;
  return: string;
  JWT_TOKEN = "JWT_TOKEN";
  APP_USER = "MATX_USER";

  constructor(
    private ls: LocalStoreService,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(
      (params) => (this.return = params["return"] || "/")
    );
  }

  public signin(username, password) {
    this.signingIn = true;
    return this.http
      .post(`${environment.apiURL}/auth/login`, { username, password })
      .pipe(
        map((res: GeneralResponse<TokenModel>) => {
          localStorage.setItem("tokens", JSON.stringify(res.data));
          var userInfo = this.jwtService.decodeToken(
            res.data?.accessToken
          ) as UserProfile;
          this.userProfile.next(userInfo);
          return res;
        }),
        catchError((error) => {
          console.log(error);
          return of(false);
        })
      );
  }
  refreshToken(payload: TokenModel) {
    return this.http.post(`${environment.apiURL}/auth/refresh-token`, payload);
  }
  deleteRefreshToken(payload: TokenModel) {
    return this.http.post(
      `${environment.apiURL}/auth/delete-refresh-token`,
      payload
    );
  }
  /*
    checkTokenIsValid is called inside constructor of
    shared/components/layouts/admin-layout/admin-layout.component.ts
  */
  public checkTokenIsValid() {
    return of(DEMO_USER).pipe(
      map((profile: User) => {
        // this.setUserAndToken(this.getJwtToken(), profile, true);
        this.signingIn = false;
        return profile;
      }),
      catchError((error) => {
        return of(error);
      })
    );

    /*
      The following code get user data and jwt token is assigned to
      Request header using token.interceptor
      This checks if the existing token is valid when app is reloaded
    */

    // return this.http.get(`${environment.apiURL}/api/users/profile`)
    //   .pipe(
    //     map((profile: User) => {
    //       this.setUserAndToken(this.getJwtToken(), profile, true);
    //       return profile;
    //     }),
    //     catchError((error) => {
    //       this.signout();
    //       return of(error);
    //     })
    //   );
  }

  public signout() {
    const localStorageTokens = localStorage.getItem("tokens");
    var token: TokenModel;
    if (localStorageTokens) {
      token = JSON.parse(localStorageTokens) as TokenModel;
      this.deleteRefreshToken(token).subscribe((res) => {
        localStorage.removeItem("tokens");
        this.router.navigateByUrl("sessions/signin");
      });
    }
  }

  public clear() {
    localStorage.removeItem("tokens");
    this.router.navigateByUrl("sessions/signin");
  }

  isLoggedIn(): Boolean {
    return !!this.getAccessToken();
  }
  getPermissions() {
    var localStorageToken = localStorage.getItem("tokens");
    if (localStorageToken == "undefined") return null;
    if (localStorageToken) {
      var token = JSON.parse(localStorageToken) as TokenModel;
      var isTokenExpired = this.jwtService.isTokenExpired(token.accessToken);
      //when token expired,request for new token
      // if (isTokenExpired) {
      //     this.refreshToken(token).pipe(
      //     switchMap((res: GeneralResponse<TokenModel>) => {
      //       localStorage.setItem("tokens", JSON.stringify(res?.data));
      //       var userInfo = this.jwtService.decodeToken(
      //         res.data.accessToken
      //       ) as UserProfile;
      //       this.userProfile.next(userInfo);
      //       return res.data.permissions;
      //     })
      //   );
      // }
      //toekn refresh end
      // if (isTokenExpired) {
      //   this.userProfile.next(null);
      //   return null;
      // }
      var userInfo = this.jwtService.decodeToken(
        token.accessToken
      ) as UserProfile;
      this.userProfile.next(userInfo);
      return token.permissions;
    }
    return null;
  }
  getAccessToken(): string {
    var localStorageToken = localStorage.getItem("tokens");
    if (localStorageToken == "undefined") return null;
    if (localStorageToken) {
      var token = JSON.parse(localStorageToken) as TokenModel;
      // var isTokenExpired = this.jwtService.isTokenExpired(token.accessToken);
      // if (isTokenExpired) {
      //   this.userProfile.next(null);
      //   return null;
      // }
      var userInfo = this.jwtService.decodeToken(
        token.accessToken
      ) as UserProfile;
      this.userProfile.next(userInfo);
      return token.accessToken;
    }
    return null;
  }

  private getDecodedToken() {
    let token = this.getAccessToken();
    // if token is undefined, avoid exception
    if (!token) {
      return null;
    }
    const jwtService = new JwtHelperService();
    const decodedToken = jwtService.decodeToken(token);
    return decodedToken;
  }
  public isRoleAuthorized(
    authorizationType: string,
    allowedData: string[]
  ): boolean {
    if (allowedData == null || allowedData.length === 0) {
      return true;
    }
    const decodeToken = this.getDecodedToken();
    // console.log("user profile", decodeToken as UserProfile);
    if (!decodeToken) {
      console.log("Invalid token");
      return false;
    }
    if (authorizationType === "Role") {
      return allowedData.includes(decodeToken.role);
    }
  }
  public isPermissionAuthorized(
    authorizationType: string,
    allowedData: string[]
  ): boolean {
    if (allowedData == null || allowedData.length === 0) {
      return false;
    }
    if (authorizationType === "Permission") {
      const permissions = this.getPermissions();
      if (
        permissions === undefined ||
        permissions === null ||
        permissions.length === 0
      )
        return false;
      return allowedData.some((a) => permissions.includes(a));
    }
  }
}
