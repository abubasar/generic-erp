import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { Category } from "../../models/category/category.model";
import { Country } from "../../models/country/country.model";
import { Generic } from "../../models/generic/generic.model";
import { InventoryType } from "../../models/inventory-type/inventory-type.model";
import { PackSize } from "../../models/pack-size/pack-size.model";
import { ProductType } from "../../models/product-type/product-type.model";
import { ProductDTO } from "../../models/product/product-dto.model";
import { ProductRequest } from "../../models/product/product-request.model";
import { ProductView } from "../../models/product/product-view.model";
import { CategoryService } from "../../services/category.service";
import { CountryService } from "../../services/country.service";
import { GenericService } from "../../services/generic.service";
import { InventoryTypeService } from "../../services/inventory-type.service";
import { PackSizeService } from "../../services/pack-size.service";
import { ProductTypeService } from "../../services/product-type.service";
import { ProductService } from "../../services/product.service";
import { ProductFormComponent } from "./product-form/product-form.component";

@Component({
  selector: "app-product",
  templateUrl: "./product.component.html",
  styleUrls: ["./product.component.scss"],
})
export class ProductComponent implements OnInit {
  loading: boolean = true;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  panelOpenState: boolean;
  businessType: string;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  dataSource: MatTableDataSource<ProductView>;
  totalCount: number;
  productRequest = new ProductRequest();
  displayedColumns$: Observable<string[]>;
  productTypes: ProductType[];
  inventoryTypes: InventoryType[];
  generics: Generic[];
  categories: Category[];
  packSizes: PackSize[];
  countries: Country[];
  mapProductTypes: ProductType[];
  pageSizeOptions: [] = Page_Size_Options;
  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "actions", label: "Actions" },
    { def: "code", label: "Code" },
    { def: "name", label: "Name" },
    { def: "genericId", label: "Generic" },
    { def: "composition", label: "Composition" },
    { def: "productTypeId", label: "Product Type" },
    { def: "categoryId", label: "Category" },
    { def: "inventoryTypeId", label: "Inventory Type" },
    { def: "countryId", label: "Country" },
    { def: "manufacturerId", label: "Manufacturer" },
    { def: "vatPercentage", label: "Vat (%)" },
    { def: "packSizeId", label: "Pack Size" },
    { def: "salePrice", label: "TP" },
    { def: "mrp", label: "MRP" },
    { def: "purchasePrice", label: "Purchase Price" },
    // { def: "bagWeight", label: "Bag Weight" },
    { def: "measurementUnitId", label: "Measurement Unit" },
    { def: "alertQuantity", label: "Alert Quantity" },
    { def: "isPurchaseProduct", label: "Is Purchase Product" },
    { def: "isSaleProduct", label: "Is Sale Product" },
  ];

  constructor(
    private productService: ProductService,
    private productTypeService: ProductTypeService,
    private genericService: GenericService,
    private categoryService: CategoryService,
    private packSizeService: PackSizeService,
    private countryService: CountryService,
    private inventoryTypeService: InventoryTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private jwtAuth: JwtAuthService,
    private confirmDialogService: ConfirmDialogService,
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    this.getProducts(this.productRequest);
    this.getAllProductTypes();
    this.getAllGenerics();
    if (this.businessType === "1") {
      this.getAllCategories();
      this.getAllPackSizes();
    }
    this.getAllCountries();
    this.getAllInventoryTypes();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      productTypeId: [null],
      inventoryTypeId: [null],
      categoryId: [null],
      genericId: [null],
      packSizeId: [null],
      countryId: [null],
      isPurchaseProduct: [false],
      isSaleProduct: [false],
      reportType: [1],
    });
    this.viewColumnForm = this.fb.group({
      code: [true],
      name: [true],
      genericId: [true],
      composition: [false],
      inventoryTypeId: [true],
      productTypeId: [true],
      categoryId: [true],
      countryId: [true],
      manufacturerId: [false],
      vatPercentage: [false],
      isPurchaseProduct: [true],
      isSaleProduct: [true],
      packSizeId: [true],
      measurementUnitId: [true],
      salePrice: [true],
      mrp: [true],
      purchasePrice: [true],
      alertQuantity: [true],
      // bagWeight: [true],
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
        .map((col) => col.def),
    );
  }

  getProducts(request: ProductRequest): void {
    this.productService.getProducts(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<ProductView>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getAllProductTypes() {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1;
    });
  }
  getAllInventoryTypes() {
    this.inventoryTypeService.getAllInventoryTypes().subscribe((res) => {
      this.inventoryTypes = res?.data?.item1;
    });
  }
  getAllCountries() {
    this.countryService.getAllCountries().subscribe((res) => {
      this.countries = res?.data?.item1;
    });
  }
  getAllGenerics() {
    this.genericService.getAllGenerics().subscribe((res) => {
      this.generics = res?.data?.item1;
    });
  }
  getAllCategories() {
    this.categoryService.getAllCategories().subscribe((res) => {
      this.categories = res?.data?.item1;
    });
  }
  getAllPackSizes() {
    this.packSizeService.getAllPackSizes().subscribe((res) => {
      this.packSizes = res?.data?.item1;
    });
  }

  onSelectedInventoryType(id: string) {
    this.mapProductTypes = this.productTypes?.filter(
      (type) => type.inventoryTypeId === id,
    );
  }

  remove(id): void {
    this.productService.deleteProduct(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getProducts(this.productRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(product?: ProductDTO): void {
    const dialogRef = this.dialog.open(ProductFormComponent, {
      disableClose: true,
      data: product,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getProducts(this.productRequest);
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
      data.message,
    );
  }

  reload() {
    this.searchForm.reset();
    this.searchForm.get("isPurchaseProduct")?.setValue(false);
    this.searchForm.get("isSaleProduct")?.setValue(false);
    this.loading = true;
    this.productRequest = new ProductRequest();
    this.getProducts(this.productRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateProductRequest();
    this.getProducts(this.productRequest);
  }

  onPageChange(pageEvent) {
    this.updateProductRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getProducts(this.productRequest);
  }

  private updateProductRequest(
    pageIndex = this.productRequest.page,
    pageSize = this.productRequest.rowsPerPage,
  ) {
    this.productRequest = {
      ...this.productRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf(buttonName) {
    if (buttonName == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if (buttonName == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    this.http
      .post(environment.apiURL + "/product/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
        if (buttonName == "pdf") {
          //Create a Blob from the PDF Stream
          const file = new Blob([response], { type: "application/pdf" });
          //Build a URL from the file
          const fileURL = URL.createObjectURL(file);
          //Open the URL on new Window
          const pdfWindow = window.open();
          pdfWindow.location.href = fileURL;
          this.isLoading1 = false;
        } else {
          const blob = new Blob([response], {
            type: "application/octet-stream",
          });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = url;
          link.setAttribute("download", "Product_List.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
