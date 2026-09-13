import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { JwtHelperService } from "@auth0/angular-jwt";
import { MeResponse } from "app/shared/models/me-response.model";
import { TokenModel } from "app/shared/models/token.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { environment } from "environments/environment";
import { BehaviorSubject, of } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { User } from "../../models/user.model";
import { LocalStoreService } from "../local-store.service";

const ME_STORAGE_KEY = "me";
const ACCESS_MODULE_PERMISSION_PREFIX = "Permissions.AccessModules.";

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
    shared/components/layouts/admin-layout/admin-layout.component.ts.
    Fetches the tenant/module/subscription profile so the sidenav's
    per-item module-entitlement check (see isModuleAuthorized below) has
    real data instead of always passing open.
  */
  public checkTokenIsValid() {
    return this.http.get<MeResponse>(`${environment.apiURL}/me`).pipe(
      map((me: MeResponse) => {
        localStorage.setItem(ME_STORAGE_KEY, JSON.stringify(me));
        this.signingIn = false;
        return me;
      }),
      catchError((error) => {
        return of(error);
      })
    );
  }

  public getMe(): MeResponse | null {
    const raw = localStorage.getItem(ME_STORAGE_KEY);
    if (!raw || raw === "undefined") return null;
    try {
      return JSON.parse(raw) as MeResponse;
    } catch {
      return null;
    }
  }

  /*
    Sibling to isPermissionAuthorized: a menu item's permission of the
    shape "Permissions.AccessModules.<X>" also names a module key (lower-
    cased). If the tenant's /api/me hasn't reported that module enabled,
    the item is hidden even if the user's role has the permission claim.
    Items whose permission isn't an AccessModules one aren't module-gated
    (they pass through) — module state gates the seven top-level sections,
    not every leaf screen.
  */
  public isModuleAuthorized(permissions: string[]): boolean {
    if (!permissions || permissions.length === 0) return true;
    const moduleKeys = permissions
      .filter((p) => p?.startsWith(ACCESS_MODULE_PERMISSION_PREFIX))
      .map((p) => p.substring(ACCESS_MODULE_PERMISSION_PREFIX.length).toLowerCase());
    if (moduleKeys.length === 0) return true;
    const me = this.getMe();
    if (!me) return true; // /api/me not loaded yet — permission check already gates real access server-side
    return moduleKeys.some((k) => me.modules?.includes(k));
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
  // Token expiry/refresh is handled by TokenInterceptor (checks
  // isTokenExpired before every request and calls refreshToken()) — this
  // method just needs the current, possibly-stale-for-a-moment token.
  getPermissions() {
    var localStorageToken = localStorage.getItem("tokens");
    if (localStorageToken == "undefined") return null;
    if (localStorageToken) {
      var token = JSON.parse(localStorageToken) as TokenModel;
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
