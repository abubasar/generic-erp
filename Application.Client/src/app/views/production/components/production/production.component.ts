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
import { Router } from "@angular/router";
import {
  Inventory_Type_Id_Finished_Goods,
  Page_Size_Options,
} from "app/shared/consts/const";
import { ProductionStatus } from "app/shared/enums/productionStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { ProductionAggregatorModel } from "../../models/production/production-aggregator.model";
import { ProductionResponseDTO } from "../../models/production/production-response-dto.model";
import { ProductionSearchRequestDTO } from "../../models/production/production-search-request-dto.model";
import { ProductionService } from "../../services/production.service";

@Component({
  selector: "app-production",
  templateUrl: "./production.component.html",
  styleUrls: ["./production.component.scss"],
  animations: [
    trigger("detailExpand", [
      state("collapsed", style({ height: "0px", minHeight: "0" })),
      state("expanded", style({ height: "*" })),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)"),
      ),
    ]),
  ],
})
export class ProductionComponent implements OnInit {
  loading: boolean = true;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<ProductionResponseDTO>;
  reportAggregator = new ProductionAggregatorModel();
  totalCount: number;
  finishedProducts: ProductView[];
  filterFinishedProducts: ProductView[];
  productionRequest = new ProductionSearchRequestDTO();
  productionStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  businessType: string;
  private path = {
    addNew: "production/production/add-new",
    edit: "production/production",
    view: "production/production/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "productionNo", label: "Production No" },
    { def: "manufacturingOrderNo", label: "Manufacturing Order No." },
    { def: "bomNo", label: "BOM No." },
    { def: "fgstore", label: "Finished Goods Store" },
    { def: "productionDate", label: "Production Date" },
    { def: "formulationNo", label: "Formulation No." },
    { def: "batchNo", label: "Batch No" },
    { def: "finishedProduct", label: "Finished Product" },
    { def: "totalRmused", label: "Total RM Used" },
    { def: "productionQuantity", label: "Production Quantity" },
    { def: "extraDamageQuantity", label: "Short/Excess Quantity" },
    { def: "dustLooseInQuantity", label: "Dust/Loose In Quantity" },
    { def: "dustLooseOutQuantity", label: "Dust/Loose Out Quantity" },
    { def: "actualProductionQuantity", label: "Actual Production Quantity" },
    { def: "rmCost", label: "RM Cost" },
    { def: "totalAdjustmentQuantity", label: "Total Adjustment Quantity" },
    { def: "totalAdjustmentCost", label: "Total Adjustment Cost" },
    { def: "totalCost", label: "Total Cost" },
    { def: "shift", label: "Shift" },
    { def: "machine", label: "Machine" },
    { def: "startDateTime", label: "Start Date Time" },
    { def: "endDateTime", label: "End Date Time" },
    { def: "breakTime", label: "Break Time" },
    { def: "status", label: "Status" },
    { def: "remark", label: "Remark" },
    { def: "checkedBy", label: "Checked By" },
    { def: "approvedBy", label: "Approved By" },
    { def: "createdOn", label: "Created On" },
    { def: "updatedOn", label: "Updated On" },
    { def: "createdBy", label: "Created By" },
    { def: "updatedBy", label: "Updated By" },
  ];

  constructor(
    private dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    private productionService: ProductionService,
    private enumValueService: EnumValueService,
    private productService: ProductService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService, // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getProductions(this.productionRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllProducts();
    this.getAllProductionStatuses();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      productionStatus: [null],
      fromDate: [null],
      toDate: [null],
      finishedProductId: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.finishedProductId === "") {
        this.searchForm
          .get("finishedProductId")
          ?.patchValue(null, { emitEvent: false });
      }
    });

    this.viewColumnForm = this.fb.group({
      expand: [true],
      productionNo: [true],
      manufacturingOrderNo: [true],
      bomNo: [false],
      fgstore: [true],
      productionDate: [true],
      formulationNo: [true],
      batchNo: [true],
      finishedProduct: [true],
      totalRmused: [true],
      productionQuantity: [true],
      extraDamageQuantity: [false],
      dustLooseInQuantity: [false],
      dustLooseOutQuantity: [false],
      actualProductionQuantity: [true],
      totalAdjustmentQuantity: [false],
      totalAdjustmentCost: [false],
      rmCost: [true],
      totalCost: [false],
      shift: [false],
      machine: [false],
      startDateTime: [false],
      endDateTime: [false],
      breakTime: [false],
      status: [true],
      remark: [false],
      checkedBy: [false],
      approvedBy: [false],
      createdOn: [false],
      updatedOn: [false],
      createdBy: [false],
      updatedBy: [false],
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

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("finishedProductId").setValue(null);
  }

  /**---------- Product Autocomplete------------ */
  onProductChange(value: string): void {
    const term = value.toLocaleLowerCase();
    this.filterProduct(term || "");
  }

  private filterProduct(value: string): void {
    this.filterFinishedProducts = this.finishedProducts?.filter((option) =>
      option.name.toLowerCase().includes(value),
    );
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
    });
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.inventoryTypeId = Inventory_Type_Id_Finished_Goods;
    productRequest.page = -1;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.filterFinishedProducts = this.finishedProducts = res?.data?.item1;
    });
  }

  getProductName(productId: string) {
    if (!productId) {
      return;
    }
    const product = this.finishedProducts?.find(
      (product) => product?.id === productId,
    );
    return product?.name;
  }
  /**---------- End Autocomplete----------- */

  getAllProductionStatuses() {
    this.enumValueService.getProductionStatuses().subscribe((res) => {
      this.productionStatuses = res;
    });
  }

  getProductionStatus(value: number) {
    return this.statusColorService.getProductionStatus(value);
  }

  getProductionStatusName(value: number) {
    return ProductionStatus[value];
  }

  getProductions(requestBody: ProductionSearchRequestDTO): void {
    this.productionService.getProductions(requestBody).subscribe((res) => {
      this.dataSource = new MatTableDataSource<ProductionResponseDTO>(
        res?.data?.item1,
      );
      this.reportAggregates(requestBody);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  reportAggregates(requestBody: ProductionSearchRequestDTO): void {
    this.productionService.reportAggregates(requestBody).subscribe((res) => {
      this.reportAggregator = res.data;
    });
  }

  getPendingCheckedCount(): void {
    this.productionService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.productionRequest.productionStatus = 1;
    this.getProductions(this.productionRequest);
  }

  getChecked() {
    this.loading = true;
    this.productionRequest.productionStatus = 2;
    this.getProductions(this.productionRequest);
  }

  remove(id: string): void {
    this.productionService.deleteProduction(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getProductions(this.productionRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
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
    this.loading = true;
    this.productionRequest = new ProductionSearchRequestDTO();
    this.getProductions(this.productionRequest);
  }

  onSearch() {
    this.loading = true;
    this.productionRequest = {
      ...this.productionRequest,
      ...this.searchForm.value,
    };
    this.getProductions(this.productionRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.productionRequest.page = pageEvent.pageIndex;
    this.productionRequest.rowsPerPage = pageEvent.pageSize;
    this.getProductions(this.productionRequest);
  }

  printPdf(reportTypeName: string, id: string) {
    let reportType = 0;
    if (reportTypeName == "pdf") {
      reportType = 1;
      this.isLoading1 = true;
    }
    if (reportTypeName == "excel") {
      reportType = 2;
      this.isLoading2 = true;
    }
    this.http
      .get(
        environment.apiURL +
          `/ReportProduction/production-bill-details/${id}/${reportType}`,
        {
          responseType: "blob",
        },
      )
      .subscribe((response) => {
        if (reportTypeName == "pdf") {
          const file = new Blob([response], { type: "application/pdf" });
          const fileURL = URL.createObjectURL(file);
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
          link.setAttribute("download", "Production_Bill_Details.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }

  printProductionSummaryPdf(id) {
    this.http
      .get(
        environment.apiURL + "/ReportProduction/production-bill-summary/" + id,
        {
          responseType: "blob",
        },
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

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
