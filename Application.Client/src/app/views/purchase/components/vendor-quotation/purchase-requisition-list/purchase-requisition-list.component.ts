import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Priority } from "app/shared/enums/priority";
import { RequisitionStatus } from "app/shared/enums/requisitionStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";

@Component({
  selector: "app-purchase-requisition-list",
  templateUrl: "./purchase-requisition-list.component.html",
  styleUrls: ["./purchase-requisition-list.component.scss"],
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
export class PurchaseRequisitionListComponent implements OnInit {
  loading: boolean = true;
  selectedRequisition: PurchaseRequisitionResponseDTO = null; // its need for single selection
  purchaseRequisitions: PurchaseRequisitionResponseDTO[];
  purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();

  panelOpenState = false;
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  requisitionStatuses: ENUM[];
  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "requisitionNo",
    "department",
    "store",
    "requisitionDate",
    "expectedDeliveryDate",
    "total",
    "requestByName",
    "priority",
    "requisitionStatus",
  ];
  dataSource: MatTableDataSource<PurchaseRequisitionResponseDTO>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public fromComponent: string,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private purchaseRequisitionService: PurchaseRequisitionService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getPurchaseRequisitions("");
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      requisitionNo: [""],
    });
  }

  getAllRequisitionStatuses() {
    this.enumValueService.getRequisitionStatuses().subscribe((res) => {
      console.log("rq status:", res);
      this.requisitionStatuses = res;
    });
  }

  getRequisitionStatus(value) {
    return this.statusColorService.getRequisitionStatus(value);
  }

  getPriority(value) {
    return this.statusColorService.getPriority(value);
  }
  getPriorityName(value: number) {
    return Priority[value];
  }

  getRequisitionStatusName(value: number) {
    return RequisitionStatus[value];
  }

  getPurchaseRequisitions(keyword): void {
    this.purchaseRequisitionRequest.requisitionStatusIds =
      this.fromComponent == "fromPurchaseOrderComponent" ? [3,4,8] : [5];
    this.purchaseRequisitionRequest.keyword = keyword;
    this.purchaseRequisitionService
      .getPurchaseRequisitions(this.purchaseRequisitionRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        // this.purchaseRequisitions = res?.data?.item1;
        this.dataSource =
          new MatTableDataSource<PurchaseRequisitionResponseDTO>(
            res?.data?.item1
          );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getPurchaseRequisitionsNo(keyword): void {
    this.purchaseRequisitionRequest.requisitionStatusIds = [5];
    this.purchaseRequisitionRequest.keyword = keyword;
    this.purchaseRequisitionService
      .getPurchaseRequisitions(this.purchaseRequisitionRequest)
      .subscribe((res) => {
        console.log(res?.data?.item1);
        this.purchaseRequisitions = res?.data?.item1;
      });
  }

  onRequisitionNoChange(): void {
    const term = this.searchForm.get("requisitionNo");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    console.log("filterValue", filterValue);
    this.getPurchaseRequisitionsNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    console.log(this.searchForm.value);
    let requisitionNo = this.searchForm.get("requisitionNo").value;
    this.getPurchaseRequisitions(requisitionNo);
  }

  onPageChange(pageEvent) {
    this.purchaseRequisitionRequest.page = pageEvent.pageIndex;
    this.purchaseRequisitionRequest.rowsPerPage = pageEvent.pageSize;
    this.getPurchaseRequisitions(this.searchForm.get("requisitionNo").value);
  }

  handleRadioChange($event) {
    this.selectedRequisition = $event.value;
  }
}
