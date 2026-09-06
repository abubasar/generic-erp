import { Injectable } from "@angular/core";
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
} from "@angular/common/http";
import { Observable } from "rxjs";
import { catchError } from "rxjs/operators";
import { ToastrService } from "ngx-toastr"; // Example: Using Toastr for displaying error messages
@Injectable()
export class TooManyRequestInterceptor implements HttpInterceptor {
  constructor(private toastr: ToastrService) {} // Example: Using Toastr for displaying error messages

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error) => {
        if (error.status === 429) {
          this.toastr.error(
            "Too many Request",
            JSON.stringify(error.error)
          );
        }
        throw error;
      })
    );
  }
}
