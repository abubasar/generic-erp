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
export class Status412PreconditionFailed implements HttpInterceptor {
  constructor(private toastr: ToastrService) {} // Example: Using Toastr for displaying error messages

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error) => {
        if (error.status === 412) {
          this.toastr.error("Precondition Failed", JSON.stringify("This Entity is already modified by someone.Please Refresh and Try Again!"));
        }
        throw error;
      })
    );
  }
}
