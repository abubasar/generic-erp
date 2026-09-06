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
import { Page_Size_Options } from "app/shared/consts/const";
import { BOMStatus } from "app/shared/enums/bomStatus";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { BillOfMaterialResponseDTO } from "../../models/bill-of-material/bill-of-material-response-dto.model";
import { BillOfMaterialSearchRequestDTO } from "../../models/bill-of-material/bill-of-material-search-request-dto.model";
import { BillOfMaterialService } from "../../services/bill-of-material.service";

@Component({
  selector: "app-bill-of-material",
  templateUrl: "./bill-of-material.component.html",
  styleUrls: ["./bill-of-material.component.scss"],
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
export class BillOfMaterialComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<BillOfMaterialResponseDTO>;
  totalCount: number;
  billOfMaterialRequest = new BillOfMaterialSearchRequestDTO();
  bomStatuses: ENUM[];
  pendingCheckedModel: PendingCheckedCount;
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  currentFinancialYearId: string;
  businessType: string;
  private path = {
    addNew: "production/bill-of-material/add-new",
    edit: "production/bill-of-material",
    view: "production/bill-of-material/view",
  };

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "bomNo", label: "BOM No." },
    { def: "finishedProduct", label: "Finished Product" },
    { def: "copiedFromBomNo", label: "Source BOM No" },
    { def: "formulationNo", label: "Formulation No" },
    { def: "dosageQuantity", label: "Dosage Quantity" },
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
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService,
    public statusColorService: StatusColorService,
    private billOfMaterialService: BillOfMaterialService,
    private enumValueService: EnumValueService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private router: Router,
    private confirmDialogService: ConfirmDialogService,
    private http: HttpClient // private localStorageService: LocalStoreService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getPendingCheckedCount();
    this.getBillOfMaterials(this.billOfMaterialRequest);
    this.getCurrentFinancialYearId();
    this.subscribeToFormChanges();
  }

  openExpansionPanel() {
    this.panelOpenState = true;
    this.getAllBOMStatuses();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      status: [null],
      fromDate: [null],
      toDate: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      bomNo: [true],
      finishedProduct: [true],
      copiedFromBomNo: [false],
      formulationNo: [true],
      dosageQuantity: [true],
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
        .map((col) => col.def)
    );
  }

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  private getCurrentFinancialYearId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.currentFinancialYearId = res.fyid;
      this.businessType = res.businesstype;
    });
  }

  getAllBOMStatuses() {
    this.enumValueService.getBOMStatuses().subscribe((res) => {
      this.bomStatuses = res;
    });
  }

  getBOMStatus(value: number) {
    return this.statusColorService.getBOMStatus(value);
  }

  getBOMStatusName(value: number) {
    return BOMStatus[value];
  }

  getBillOfMaterials(requestBody: BillOfMaterialSearchRequestDTO): void {
    this.billOfMaterialService
      .getBillOfMaterials(requestBody)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<BillOfMaterialResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPendingCheckedCount(): void {
    this.billOfMaterialService.getPendingCheckedCount().subscribe((res) => {
      this.pendingCheckedModel = res.data;
    });
  }

  getPending() {
    this.loading = true;
    this.billOfMaterialRequest.bomStatus = 1;
    this.getBillOfMaterials(this.billOfMaterialRequest);
  }

  getChecked() {
    this.loading = true;
    this.billOfMaterialRequest.bomStatus = 2;
    this.getBillOfMaterials(this.billOfMaterialRequest);
  }

  remove(id: string): void {
    this.billOfMaterialService.deleteBillOfMaterial(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getBillOfMaterials(this.billOfMaterialRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  generateReport(event: Event, id: number) {
    const targetName = (event.currentTarget as HTMLButtonElement).name;
    const reportType = targetName === "pdf" ? 1 : 2;

    const endpoint =
      environment.apiURL + `/ReportBillOfMaterial/${id}/${reportType}`;

    this.http
      .get(endpoint, { responseType: "blob" })
      .subscribe((response) => this.handleFileResponse(response, targetName));
  }

  private handleFileResponse(response: Blob, targetName: string): void {
    const fileType =
      targetName === "pdf" ? "application/pdf" : "application/octet-stream";
    // Create a Blob from the PDF Stream
    const file = new Blob([response], { type: fileType });
    const fileURL = URL.createObjectURL(file);
    if (targetName === "pdf") {
      // Open the URL in a new window
      const pdfWindow = window.open();
      pdfWindow.location.href = fileURL;
    } else {
      const link = document.createElement("a");
      link.href = fileURL;
      // Generate a dynamic filename based on the report type
      link.setAttribute("download", `Bill_Of_Material.xlsx`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link); // Cleanup after download
    }
  }

  printBillOfMaterialPdf(event: Event, id: number) {
    this.generateReport(event, id);
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
    this.searchForm.reset();
    this.loading = true;
    this.billOfMaterialRequest = new BillOfMaterialSearchRequestDTO();
    this.getBillOfMaterials(this.billOfMaterialRequest);
  }

  onSearch() {
    this.loading = true;
    this.billOfMaterialRequest.keyword = this.searchForm.value.keyword;
    this.billOfMaterialRequest.bomStatus = this.searchForm.value.status;
    this.billOfMaterialRequest.fromDate = this.searchForm.value.fromDate;
    this.billOfMaterialRequest.toDate = this.searchForm.value.toDate;
    this.getBillOfMaterials(this.billOfMaterialRequest);
  }

  navigateToAddNew() {
    this.router.navigate([this.path.addNew]);
  }

  navigateToView(item) {
    this.router.navigate([this.path.edit, item.id]);
    // this.localStorageService.setItem(item.id, item);
  }

  onPageChange(pageEvent) {
    this.billOfMaterialRequest.page = pageEvent.pageIndex;
    this.billOfMaterialRequest.rowsPerPage = pageEvent.pageSize;
    this.getBillOfMaterials(this.billOfMaterialRequest);
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
