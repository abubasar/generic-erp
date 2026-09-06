import { Routes } from "@angular/router";

import { SigninComponent } from "./signin/signin.component";
import { SignupComponent } from "./signup/signup.component";
import { NotFoundComponent } from "./not-found/not-found.component";
import { AccessDeniedComponent } from "./access-denied/access-denied.component";

export const SessionsRoutes: Routes = [
  {
    path: "",
    children: [
      {
        path: "signup",
        component: SignupComponent,
        data: { title: "Signup" },
      },
      {
        path: "signin",
        component: SigninComponent,
        data: { title: "Signin" },
      },
      {
        path: "access-denied",
        component: AccessDeniedComponent,
        data: { title: "Not Found" },
      },
      {
        path: "404",
        component: NotFoundComponent,
        data: { title: "Not Found" },
      },
    ],
  },
];
