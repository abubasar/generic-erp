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
import { CostCenterRequest } from "../../models/cost-center/cost-center-request.model";
import { CostCenter } from "../../models/cost-center/cost-center.model";
import { CostCenterService } from "../../services/cost-center.service";
import { CostCenterFormComponent } from "./cost-center-form/cost-center-form.component";

@Component({
  selector: "app-cost-center",
  templateUrl: "./cost-center.component.html",
  styleUrls: ["./cost-center.component.scss"],
})
export class CostCenterComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"]; //"code",
  dataSource: MatTableDataSource<CostCenter>;
  totalCount: number;
  costCenterRequest = new CostCenterRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private costCenterService: CostCenterService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getCostCenters(this.costCenterRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getCostCenters(request: CostCenterRequest): void {
    this.costCenterService.getCostCenters(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<CostCenter>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.costCenterService.deleteCostCenter(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCostCenters(this.costCenterRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(costCenter?: CostCenter): void {
    const dialogRef = this.dialog.open(CostCenterFormComponent, {
      disableClose: true,
      data: costCenter,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCostCenters(this.costCenterRequest);
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
    this.costCenterRequest = new CostCenterRequest();
    this.getCostCenters(this.costCenterRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCostCenterRequest();
    this.getCostCenters(this.costCenterRequest);
  }

  onPageChange(pageEvent) {
    this.updateCostCenterRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCostCenters(this.costCenterRequest);
  }

  private updateCostCenterRequest(
    pageIndex = this.costCenterRequest.page,
    pageSize = this.costCenterRequest.rowsPerPage
  ) {
    this.costCenterRequest = {
      ...this.costCenterRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/CostCenter/print", this.searchForm.value, {
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
