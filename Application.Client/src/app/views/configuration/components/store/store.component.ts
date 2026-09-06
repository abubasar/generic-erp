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
import { InventoryType } from "../../models/inventory-type/inventory-type.model";
import { StoreRequest } from "../../models/store/store-request.model";
import { Store } from "../../models/store/store.model";
import { InventoryTypeService } from "../../services/inventory-type.service";
import { StoreService } from "../../services/store.service";
import { StoreFormComponent } from "./store-form/store-form.component";

@Component({
  selector: "app-store",
  templateUrl: "./store.component.html",
  styleUrls: ["./store.component.scss"],
})
export class StoreComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "actions",
    "code",
    "name",
    "depoChargePerKg",
    "inventoryTypeId",
  ];
  dataSource: MatTableDataSource<Store>;
  totalCount: number;
  storeRequest = new StoreRequest();
  inventoryTypes: InventoryType[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private storeService: StoreService,
    private inventoryTypeService: InventoryTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getStores(this.storeRequest);
    this.getAllInventoryTypes();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      inventoryTypeId: [null],
    });
  }

  getStores(request: StoreRequest): void {
    this.storeService.getStores(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Store>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getAllInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }

  remove(id): void {
    this.storeService.deleteStore(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getStores(this.storeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(store?: Store): void {
    const dialogRef = this.dialog.open(StoreFormComponent, {
      disableClose: true,
      data: store,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getStores(this.storeRequest);
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
    this.storeRequest = new StoreRequest();
    this.getStores(this.storeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateStoreRequest();
    this.getStores(this.storeRequest);
  }

  onPageChange(pageEvent) {
    this.updateStoreRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getStores(this.storeRequest);
  }

  private updateStoreRequest(
    pageIndex = this.storeRequest.page,
    pageSize = this.storeRequest.rowsPerPage
  ) {
    this.storeRequest = {
      ...this.storeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Store/print", this.searchForm.value, {
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
