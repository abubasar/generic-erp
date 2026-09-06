import { inject } from "@angular/core";
import { JwtAuthService } from "../services/auth/jwt-auth.service";
import { Router } from "@angular/router";

export const hasPermission = (accessList: string[]) => {
  const jwtAuth = inject(JwtAuthService);
  const router = inject(Router);
  if (jwtAuth.isLoggedIn()) {
    const isAuthorized = jwtAuth.isPermissionAuthorized("Permission",accessList);
    if (isAuthorized) return true;
    else {
      router.navigate(["/sessions/access-denied"]);
      return false;
    }
  } else {
    router.navigate(["/sessions/signin"]);
    return false;
  }
};
