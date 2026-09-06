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
import { InventoryTypeRequest } from "../../models/inventory-type/inventory-type-request.model";
import { InventoryType } from "../../models/inventory-type/inventory-type.model";
import { InventoryTypeService } from "../../services/inventory-type.service";
import { InventoryTypeFormComponent } from "./inventory-type-form/inventory-type-form.component";

@Component({
  selector: "app-inventory-type",
  templateUrl: "./inventory-type.component.html",
  styleUrls: ["./inventory-type.component.scss"],
})
export class InventoryTypeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<InventoryType>;
  totalCount: number;
  inventoryTypeRequest = new InventoryTypeRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private inventoryTypeService: InventoryTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getInventoryTypes(this.inventoryTypeRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getInventoryTypes(request: InventoryTypeRequest): void {
    this.inventoryTypeService.getInventoryTypes(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<InventoryType>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.inventoryTypeService.deleteInventoryType(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getInventoryTypes(this.inventoryTypeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(inventoryType?: InventoryType): void {
    const dialogRef = this.dialog.open(InventoryTypeFormComponent, {
      disableClose: true,
      data: inventoryType,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getInventoryTypes(this.inventoryTypeRequest);
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
    this.inventoryTypeRequest = new InventoryTypeRequest();
    this.getInventoryTypes(this.inventoryTypeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateInventoryTypeRequest();
    this.getInventoryTypes(this.inventoryTypeRequest);
  }

  onPageChange(pageEvent) {
    this.updateInventoryTypeRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getInventoryTypes(this.inventoryTypeRequest);
  }

  private updateInventoryTypeRequest(
    pageIndex = this.inventoryTypeRequest.page,
    pageSize = this.inventoryTypeRequest.rowsPerPage
  ) {
    this.inventoryTypeRequest = {
      ...this.inventoryTypeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/InventoryType/print",
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
