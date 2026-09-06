import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { ProductType } from "../../models/product-type/product-type.model";
import { ProductView } from "../../models/product/product-view.model";
import { SupplierRequest } from "../../models/supplier/supplier-request.model";
import { Supplier } from "../../models/supplier/supplier.model";
import { ProductTypeService } from "../../services/product-type.service";
import { ProductService } from "../../services/product.service";
import { SupplierService } from "../../services/supplier.service";
import { SupplierFormComponent } from "./supplier-form/supplier-form.component";

@Component({
  selector: "app-supplier",
  templateUrl: "./supplier.component.html",
  styleUrls: ["./supplier.component.scss"],
  animations: [
    trigger("detailExpand", [
      state("collapsed", style({ height: "0px", minHeight: "0" })),
      state("expanded", style({ height: "*" })),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)")
      ),
    ]),
  ],
})
export class SupplierComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<Supplier>;
  totalCount: number;
  products: ProductView[];
  supplierRequest = new SupplierRequest();
  displayedColumns$: Observable<string[]>;
  productTypes: ProductType[];
  pageSizeOptions: [] = Page_Size_Options;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "name", label: "Name" },
    { def: "ownersName", label: "Owner's Name" },
    { def: "contactNo", label: "Contact No" },
    { def: "email", label: "Email" },
    { def: "address", label: "Address" },
    { def: "nationalId", label: "NID" },
    { def: "tradeLicense", label: "Trade License" },
    { def: "contactPersonName", label: "Contact-Person Name" },
    { def: "contactPersonEmail", label: "Contact-Person Email" },
    { def: "contactPersonContactNo", label: "Contact-Person Contact No" },
    {
      def: "contactPersonDesignation",
      label: "Contact Person Designation",
    },
    { def: "suppliedProductIds", label: "Supplier Product" },
  ];

  constructor(
    private supplierService: SupplierService,
    private productService: ProductService,
    private productTypeService: ProductTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getSuppliers(this.supplierRequest);
    this.getProducts();
    this.getAllProductTypes();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      code: [true],
      name: [true],
      ownersName: [true],
      contactNo: [true],
      email: [true],
      address: [true],
      nationalId: [false],
      tradeLicense: [false],
      contactPersonName: [false],
      contactPersonContactNo: [false],
      contactPersonEmail: [false],
      contactPersonDesignation: [false],
      suppliedProductIds: [true],
      actions: [true],
    });
    this.updateDisplayedColumns();
  }

  private subscribeToFormChanges() {
    merge(this.viewColumnForm.valueChanges).subscribe(() => {
      this.updateDisplayedColumns();
    });
  }

  private updateDisplayedColumns() {
    this.displayedColumns$ = of(
      this.columnDefinitions
        .filter((x) => this.viewColumnForm.get(x.def).value)
        .map((col) => col.def)
    );
  }

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  getSuppliers(request): void {
    this.supplierService.getSuppliers(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Supplier>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getAllProductTypes() {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1;
    });
  }

  getProducts(): void {
    this.productService.getAllProducts().subscribe((res) => {
      this.products = res?.data?.item1;
    });
  }

  getProductById(id) {
    const product = this.products?.find((p) => p.id === id);
    return product ? product?.name : null;
  }

  getProductNamesByIds(productIds) {
    const productNames = productIds?.map((id) => this.getProductById(id));
    return productNames;
  }

  remove(id): void {
    this.supplierService.deleteSupplier(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getSuppliers(this.supplierRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(supplier?: Supplier): void {
    const dialogRef = this.dialog.open(SupplierFormComponent, {
      disableClose: true,
      data: supplier,
    });
    dialogRef.afterClosed().subscribe((result) => {
      console.log(result);
      if (result) {
        this.loading = true;
        this.getSuppliers(this.supplierRequest);
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

  // onSearch() {
  //   this.supplierRequest.keyword = this.searchForm.value.keyword;
  //   this.getSuppliers(this.supplierRequest);
  // }

  // onPageChange(pageEvent) {
  //   this.supplierRequest.page = pageEvent.pageIndex;
  //   this.supplierRequest.rowsPerPage = pageEvent.pageSize;
  //   this.getSuppliers(this.supplierRequest);
  // }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.supplierRequest = new SupplierRequest();
    this.getSuppliers(this.supplierRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateSupplierRequest();
    this.getSuppliers(this.supplierRequest);
  }

  onPageChange(pageEvent) {
    this.updateSupplierRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getSuppliers(this.supplierRequest);
  }

  private updateSupplierRequest(
    pageIndex = this.supplierRequest.page,
    pageSize = this.supplierRequest.rowsPerPage
  ) {
    this.supplierRequest = {
      ...this.supplierRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Supplier/print", this.searchForm.value, {
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
