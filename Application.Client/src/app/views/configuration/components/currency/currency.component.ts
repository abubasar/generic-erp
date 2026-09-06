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
import { CurrencyRequest } from "../../models/currency/currency-request.model";
import { Currency } from "../../models/currency/currency.model";
import { CurrencyService } from "../../services/currency.service";
import { CurrencyFormComponent } from "./currency-form/currency-form.component";

@Component({
  selector: "app-currency",
  templateUrl: "./currency.component.html",
  styleUrls: ["./currency.component.scss"],
})
export class CurrencyComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Currency>;
  totalCount: number;
  currencyRequest = new CurrencyRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private currencyService: CurrencyService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getCurrencies(this.currencyRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
  }

  getCurrencies(request: CurrencyRequest): void {
    this.currencyService.getCurrencies(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Currency>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.currencyService.deleteCurrency(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCurrencies(this.currencyRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(currency?: Currency): void {
    const dialogRef = this.dialog.open(CurrencyFormComponent, {
      disableClose: true,
      data: currency,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCurrencies(this.currencyRequest);
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
    this.currencyRequest = new CurrencyRequest();
    this.getCurrencies(this.currencyRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCurrencyRequest();
    this.getCurrencies(this.currencyRequest);
  }

  onPageChange(pageEvent) {
    this.updateCurrencyRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCurrencies(this.currencyRequest);
  }

  private updateCurrencyRequest(
    pageIndex = this.currencyRequest.page,
    pageSize = this.currencyRequest.rowsPerPage
  ) {
    this.currencyRequest = {
      ...this.currencyRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Currency/print", this.searchForm.value, {
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
