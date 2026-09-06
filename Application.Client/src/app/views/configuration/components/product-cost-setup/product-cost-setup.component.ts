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
import { ProductCostSetupRequest } from "../../models/product-cost-setup/product-cost-setup-request.model";
import { ProductCostSetup } from "../../models/product-cost-setup/product-cost-setup.model";
import { ProductCostSetupService } from "../../services/product-cost-setup.service";
import { ProductCostSetupFormComponent } from "./product-cost-setup-form/product-cost-setup-form.component";

@Component({
  selector: "app-product-cost-setup",
  templateUrl: "./product-cost-setup.component.html",
  styleUrls: ["./product-cost-setup.component.scss"],
})
export class ProductCostSetupComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "actions",
    "productName",
    "directExpense",
    "factoryOverhead",
  ];
  dataSource: MatTableDataSource<ProductCostSetup>;
  totalCount: number;
  productCostSetupRequest = new ProductCostSetupRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private productCostSetupService: ProductCostSetupService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getProductCostSetups(this.productCostSetupRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getProductCostSetups(request: ProductCostSetupRequest): void {
    this.productCostSetupService
      .getProductCostSetups(request)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<ProductCostSetup>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  remove(id): void {
    this.productCostSetupService.deleteProductCostSetup(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getProductCostSetups(this.productCostSetupRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(productCostSetup?: ProductCostSetup): void {
    const dialogRef = this.dialog.open(ProductCostSetupFormComponent, {
      disableClose: true,
      data: productCostSetup,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getProductCostSetups(this.productCostSetupRequest);
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
    this.productCostSetupRequest = new ProductCostSetupRequest();
    this.getProductCostSetups(this.productCostSetupRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateProductCostSetupRequest();
    console.log(this.productCostSetupRequest);
    this.getProductCostSetups(this.productCostSetupRequest);
  }

  onPageChange(pageEvent) {
    this.updateProductCostSetupRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getProductCostSetups(this.productCostSetupRequest);
  }

  private updateProductCostSetupRequest(
    pageIndex = this.productCostSetupRequest.page,
    pageSize = this.productCostSetupRequest.rowsPerPage
  ) {
    this.productCostSetupRequest = {
      ...this.productCostSetupRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/ProductCostSetup/print",
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
