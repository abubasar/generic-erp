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
import { MeasurementUnitRequest } from "../../models/measurement-unit/measurement-unit-request.model";
import { MeasurementUnit } from "../../models/measurement-unit/measurement-unit.model";
import { MeasurementUnitService } from "../../services/measurement-unit.service";
import { MeasurementUnitFormComponent } from "./measurement-unit-form/measurement-unit-form.component";

@Component({
  selector: "app-measurement-unit",
  templateUrl: "./measurement-unit.component.html",
  styleUrls: ["./measurement-unit.component.scss"],
})
export class MeasurementUnitComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<MeasurementUnit>;
  totalCount: number;
  measurementUnitRequest = new MeasurementUnitRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private measurementUnitService: MeasurementUnitService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getMeasurementUnits(this.measurementUnitRequest);
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getMeasurementUnits(request: MeasurementUnitRequest): void {
    this.measurementUnitService
      .getMeasurementUnits(request)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<MeasurementUnit>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  remove(id): void {
    this.measurementUnitService.deleteMeasurementUnit(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getMeasurementUnits(this.measurementUnitRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(measurementUnit?: MeasurementUnit): void {
    const dialogRef = this.dialog.open(MeasurementUnitFormComponent, {
      disableClose: true,
      data: measurementUnit,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getMeasurementUnits(this.measurementUnitRequest);
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
    this.measurementUnitRequest = new MeasurementUnitRequest();
    this.getMeasurementUnits(this.measurementUnitRequest);
  }
  onSearch() {
    this.loading = true;
    this.updateMeasurementUnitRequest();
    this.getMeasurementUnits(this.measurementUnitRequest);
  }

  onPageChange(pageEvent) {
    this.updateMeasurementUnitRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getMeasurementUnits(this.measurementUnitRequest);
  }

  private updateMeasurementUnitRequest(
    pageIndex = this.measurementUnitRequest.page,
    pageSize = this.measurementUnitRequest.rowsPerPage
  ) {
    this.measurementUnitRequest = {
      ...this.measurementUnitRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/MeasurementUnit/print",
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
