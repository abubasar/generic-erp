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
import { ProductTypeRequest } from "../../models/product-type/product-type-request.model";
import { ProductType } from "../../models/product-type/product-type.model";
import { InventoryTypeService } from "../../services/inventory-type.service";
import { ProductTypeService } from "../../services/product-type.service";
import { ProductTypeFormComponent } from "./product-type-form/product-type-form.component";

@Component({
  selector: "app-product-type",
  templateUrl: "./product-type.component.html",
  styleUrls: ["./product-type.component.scss"],
})
export class ProductTypeComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "inventoryTypeId"];
  dataSource: MatTableDataSource<ProductType>;
  totalCount: number;
  productTypeRequest = new ProductTypeRequest();
  inventoryTypes: InventoryType[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private productTypeService: ProductTypeService,
    private inventoryTypeService: InventoryTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getProductTypes(this.productTypeRequest);
    this.getInventoryTypes();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      inventoryTypeId: [null],
    });
  }

  getProductTypes(request: ProductTypeRequest): void {
    this.productTypeService
      .getProductTypes(this.productTypeRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<ProductType>(res?.data?.item1);
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }

  remove(id): void {
    this.productTypeService.deleteProductType(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getProductTypes(this.productTypeRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(inventoryType?: ProductType): void {
    const dialogRef = this.dialog.open(ProductTypeFormComponent, {
      disableClose: true,
      data: inventoryType,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getProductTypes(this.productTypeRequest);
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
    this.productTypeRequest = new ProductTypeRequest();
    this.getProductTypes(this.productTypeRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateProductTypeRequest();
    this.getProductTypes(this.productTypeRequest);
  }

  onPageChange(pageEvent) {
    this.updateProductTypeRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getProductTypes(this.productTypeRequest);
  }

  private updateProductTypeRequest(
    pageIndex = this.productTypeRequest.page,
    pageSize = this.productTypeRequest.rowsPerPage
  ) {
    this.productTypeRequest = {
      ...this.productTypeRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/ProductType/print", this.searchForm.value, {
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
