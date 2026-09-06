import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { FinancialYearRequest } from "../../models/financial-year/financial-year-request.model";
import { FinancialYear } from "../../models/financial-year/financial-year.model";
import { FinancialYearService } from "../../services/financial-year.service";
import { FinancialYearFormComponent } from "./financial-year-form/financial-year-form.component";

@Component({
  selector: "app-financial-year",
  templateUrl: "./financial-year.component.html",
  styleUrls: ["./financial-year.component.scss"],
})
export class FinancialYearComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "actions",
    "name",
    "startDate",
    "endDate",
    "isActive",
  ];
  dataSource: MatTableDataSource<FinancialYear>;
  totalCount: number;
  financialYearRequest = new FinancialYearRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    public dateFormatService: DateTimeFormatService,
    private financialYearService: FinancialYearService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getFinancialYears(this.financialYearRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getFinancialYears(request: FinancialYearRequest): void {
    this.financialYearService.getFinancialYears(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<FinancialYear>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.financialYearService.deleteFinancialYear(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getFinancialYears(this.financialYearRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(financialYear?: FinancialYear): void {
    const dialogRef = this.dialog.open(FinancialYearFormComponent, {
      disableClose: true,
      data: financialYear,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getFinancialYears(this.financialYearRequest);
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
    this.financialYearRequest = new FinancialYearRequest();
    this.getFinancialYears(this.financialYearRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateFinancialYearRequest();
    this.getFinancialYears(this.financialYearRequest);
  }

  onPageChange(pageEvent) {
    this.updateFinancialYearRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getFinancialYears(this.financialYearRequest);
  }

  private updateFinancialYearRequest(
    pageIndex = this.financialYearRequest.page,
    pageSize = this.financialYearRequest.rowsPerPage
  ) {
    this.financialYearRequest = {
      ...this.financialYearRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/FinancialYear/print",
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
