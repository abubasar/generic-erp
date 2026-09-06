import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { PaymentMethodRequest } from "../../models/payment-method/payment-method-request.model";
import { PaymentMethod } from "../../models/payment-method/payment-method.model";
import { PaymentMethodService } from "../../services/payment-method.service";
import { PaymentMethodFormComponent } from "./payment-method-form/payment-method-form.component";

@Component({
  selector: "app-payment-method",
  templateUrl: "./payment-method.component.html",
  styleUrls: ["./payment-method.component.scss"],
})
export class PaymentMethodComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<PaymentMethod>;
  totalCount: number;
  paymentMethodRequest = new PaymentMethodRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private paymentMethodService: PaymentMethodService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getPaymentMethods(this.paymentMethodRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getPaymentMethods(request: PaymentMethodRequest): void {
    this.paymentMethodService.getPaymentMethods(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<PaymentMethod>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.paymentMethodService.deletePaymentMethod(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPaymentMethods(this.paymentMethodRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(paymentMethod?: PaymentMethod): void {
    const dialogRef = this.dialog.open(PaymentMethodFormComponent, {
      disableClose: true,
      data: paymentMethod,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getPaymentMethods(this.paymentMethodRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.paymentMethodRequest = new PaymentMethodRequest();
    this.getPaymentMethods(this.paymentMethodRequest);
  }

  onSearch() {
    this.loading = true;
    this.updatePaymentMethodRequest();
    this.getPaymentMethods(this.paymentMethodRequest);
  }

  onPageChange(pageEvent) {
    this.updatePaymentMethodRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getPaymentMethods(this.paymentMethodRequest);
  }

  private updatePaymentMethodRequest(
    pageIndex = this.paymentMethodRequest.page,
    pageSize = this.paymentMethodRequest.rowsPerPage
  ) {
    this.paymentMethodRequest = {
      ...this.paymentMethodRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/PaymentMethod/print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
