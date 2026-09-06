import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { ToastrService } from "ngx-toastr";
import { FundTransferTransactionTypeRequest } from "../../models/fund-transfer-transaction-type/fund-transfer-transaction-type-request.model";
import { FundTransferTransactionType } from "../../models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { FundTransferTransactionTypeService } from "../../services/fund-transfer-transaction-type.service";
import { FundTransferTransactionTypeFormComponent } from "./fund-transfer-transaction-type-form/fund-transfer-transaction-type-form.component";

@Component({
  selector: "app-fund-transfer-transaction-type",
  templateUrl: "./fund-transfer-transaction-type.component.html",
  styleUrls: ["./fund-transfer-transaction-type.component.scss"],
})
export class FundTransferTransactionTypeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<FundTransferTransactionType>;
  totalCount: number;
  fundTransferTransactionTypeRequest = new FundTransferTransactionTypeRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private fundTransferTransactionTypeService: FundTransferTransactionTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getFundTransferTransactionTypes(
      this.fundTransferTransactionTypeRequest
    );
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getFundTransferTransactionTypes(
    request: FundTransferTransactionTypeRequest
  ): void {
    this.fundTransferTransactionTypeService
      .getFundTransferTransactionTypes(request)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<FundTransferTransactionType>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  remove(id): void {
    this.fundTransferTransactionTypeService
      .deleteFundTransferTransactionType(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getFundTransferTransactionTypes(
            this.fundTransferTransactionTypeRequest
          );
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }
  openForm(fundTransferTransactionType?: FundTransferTransactionType): void {
    const dialogRef = this.dialog.open(
      FundTransferTransactionTypeFormComponent,
      {
        disableClose: true,
        data: fundTransferTransactionType,
      }
    );
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getFundTransferTransactionTypes(
          this.fundTransferTransactionTypeRequest
        );
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
    this.fundTransferTransactionTypeRequest =
      new FundTransferTransactionTypeRequest();
    this.getFundTransferTransactionTypes(
      this.fundTransferTransactionTypeRequest
    );
  }

  onSearch() {
    this.loading = true;
    this.updateFundTransferTransactionTypeRequest();
    this.getFundTransferTransactionTypes(
      this.fundTransferTransactionTypeRequest
    );
  }

  onPageChange(pageEvent) {
    this.updateFundTransferTransactionTypeRequest(
      pageEvent.pageIndex,
      pageEvent.pageSize
    );
    this.getFundTransferTransactionTypes(
      this.fundTransferTransactionTypeRequest
    );
  }

  private updateFundTransferTransactionTypeRequest(
    pageIndex = this.fundTransferTransactionTypeRequest.page,
    pageSize = this.fundTransferTransactionTypeRequest.rowsPerPage
  ) {
    this.fundTransferTransactionTypeRequest = {
      ...this.fundTransferTransactionTypeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  // printPdf() {
  //   this.http
  //     .post(
  //       environment.apiURL + "/FundTransferTransactionType/print",
  //       this.searchForm.value,
  //       {
  //         responseType: "blob",
  //       }
  //     )
  //     .subscribe((response) => {
  //       //Create a Blob from the PDF Stream
  //       const file = new Blob([response], { type: "application/pdf" });
  //       //Build a URL from the file
  //       const fileURL = URL.createObjectURL(file);
  //       //Open the URL on new Window
  //       const pdfWindow = window.open();
  //       pdfWindow.location.href = fileURL;
  //     });
  // }
}
