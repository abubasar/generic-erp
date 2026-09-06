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
import { PackSizeRequest } from "../../models/pack-size/pack-size-request.model";
import { PackSize } from "../../models/pack-size/pack-size.model";
import { PackSizeService } from "../../services/pack-size.service";
import { PackSizeFormComponent } from "./pack-size-form/pack-size-form.component";

@Component({
  selector: "app-pack-size",
  templateUrl: "./pack-size.component.html",
  styleUrls: ["./pack-size.component.scss"],
})
export class PackSizeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"]; //"code",
  dataSource: MatTableDataSource<PackSize>;
  totalCount: number;
  packSizeRequest = new PackSizeRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private packSizeService: PackSizeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getPackSizes(this.packSizeRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getPackSizes(request: PackSizeRequest): void {
    this.packSizeService.getPackSizes(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<PackSize>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.packSizeService.deletePackSize(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getPackSizes(this.packSizeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(packSize?: PackSize): void {
    const dialogRef = this.dialog.open(PackSizeFormComponent, {
      disableClose: true,
      data: packSize,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getPackSizes(this.packSizeRequest);
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
    this.packSizeRequest = new PackSizeRequest();
    this.getPackSizes(this.packSizeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updatePackSizeRequest();
    this.getPackSizes(this.packSizeRequest);
  }

  onPageChange(pageEvent) {
    this.updatePackSizeRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getPackSizes(this.packSizeRequest);
  }

  private updatePackSizeRequest(
    pageIndex = this.packSizeRequest.page,
    pageSize = this.packSizeRequest.rowsPerPage
  ) {
    this.packSizeRequest = {
      ...this.packSizeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }

  printPdf() {
    this.http
      .post(environment.apiURL + "/PackSize/print", this.searchForm.value, {
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
