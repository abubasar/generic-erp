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
import { PaymentModeRequest } from "../../models/payment-mode/payment-mode-request.model";
import { PaymentMode } from "../../models/payment-mode/payment-mode.model";
import { PaymentModeService } from "../../services/payment-mode.service";
import { PaymentModeFormComponent } from "./payment-mode-form/payment-mode-form.component";

@Component({
  selector: "app-payment-mode",
  templateUrl: "./payment-mode.component.html",
  styleUrls: ["./payment-mode.component.scss"],
})
export class PaymentModeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<PaymentMode>;
  totalCount: number;
  paymentModeRequest = new PaymentModeRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private paymentModeService: PaymentModeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getPaymentModes(this.paymentModeRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getPaymentModes(request: PaymentModeRequest): void {
    this.paymentModeService.getPaymentModes(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<PaymentMode>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.paymentModeService.deletePaymentMode(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPaymentModes(this.paymentModeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(paymentMode?: PaymentMode): void {
    const dialogRef = this.dialog.open(PaymentModeFormComponent, {
      disableClose: true,
      data: paymentMode,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getPaymentModes(this.paymentModeRequest);
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
    this.paymentModeRequest = new PaymentModeRequest();
    this.getPaymentModes(this.paymentModeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updatePaymentModeRequest();
    this.getPaymentModes(this.paymentModeRequest);
  }

  onPageChange(pageEvent) {
    this.updatePaymentModeRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getPaymentModes(this.paymentModeRequest);
  }

  private updatePaymentModeRequest(
    pageIndex = this.paymentModeRequest.page,
    pageSize = this.paymentModeRequest.rowsPerPage
  ) {
    this.paymentModeRequest = {
      ...this.paymentModeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/PaymentMode/print", this.searchForm.value, {
        responseType: "blob",
      })
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
