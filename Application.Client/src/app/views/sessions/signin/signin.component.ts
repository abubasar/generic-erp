import {
  Component,
  OnInit,
  ViewChild,
  OnDestroy,
  AfterViewInit,
} from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { MatButton } from "@angular/material/button";
import { MatProgressBar } from "@angular/material/progress-bar";
import {
  Validators,
  UntypedFormGroup,
  UntypedFormControl,
} from "@angular/forms";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { AppLoaderService } from "../../../shared/services/app-loader/app-loader.service";
import { JwtAuthService } from "../../../shared/services/auth/jwt-auth.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { TokenModel } from "app/shared/models/token.model";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-signin",
  templateUrl: "./signin.component.html",
  styleUrls: ["./signin.component.css"],
})
export class SigninComponent implements OnInit, OnDestroy {
  @ViewChild(MatProgressBar) progressBar: MatProgressBar;
  @ViewChild(MatButton) submitButton: MatButton;

  signinForm: UntypedFormGroup;
  errorMsg = "";

  private _unsubscribeAll: Subject<any>;

  constructor(
    private jwtAuth: JwtAuthService,
    private matxLoader: AppLoaderService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {
    this._unsubscribeAll = new Subject();
  }

  ngOnInit() {
    this.signinForm = new UntypedFormGroup({
      username: new UntypedFormControl("", Validators.required),
      password: new UntypedFormControl("", Validators.required),
      rememberMe: new UntypedFormControl(true),
    });
  }

  ngOnDestroy() {
    this._unsubscribeAll.next(1);
    this._unsubscribeAll.complete();
  }

  signin() {
    const signinData = this.signinForm.value;

    this.submitButton.disabled = true;
    this.progressBar.mode = "indeterminate";

    this.jwtAuth.signin(signinData.username, signinData.password).subscribe(
      (response: GeneralResponse<TokenModel>) => {
        if (response?.succeeded) {
          this.router.navigateByUrl(this.jwtAuth.return);
        } else {
          this.toastr.error(response.message);
          this.submitButton.disabled = false;
          this.progressBar.mode = "determinate";
        }
      },
      (err) => {
        this.submitButton.disabled = false;
        this.progressBar.mode = "determinate";
        this.errorMsg = err.message;
        // console.log(err);
      }
    );
  }
}
