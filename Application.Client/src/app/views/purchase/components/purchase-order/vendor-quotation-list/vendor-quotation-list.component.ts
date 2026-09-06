import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { VendorQuotationResponseDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-response-dto.model";
import { VendorQuotationSearchRequestDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-search-request-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";
import { VendorQuotationService } from "app/views/purchase/services/vendor-quotation.service";

@Component({
  selector: "app-vendor-quotation-list",
  templateUrl: "./vendor-quotation-list.component.html",
  styleUrls: ["./vendor-quotation-list.component.scss"],
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
export class VendorQuotationListComponent implements OnInit {
  loading: boolean = true;
  selectedVendorQuotations: VendorQuotationResponseDTO = null;
  purchaseRequisitions: PurchaseRequisitionResponseDTO[];
  vendorQuotationRequest = new VendorQuotationSearchRequestDTO();

  panelOpenState: boolean;
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  totalCount: number;

  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "quotationNo",
    "requisitionNo",
    "referenceNo",
    "supplierId",
    "transport",
    "paymentTermInDays",
    "deliveryTermInDays",
    "deliveryDate",
    "totalAmount",
    "remark",
  ];
  dataSource: MatTableDataSource<VendorQuotationResponseDTO>;

  constructor(
    public dateFormatService: DateTimeFormatService,
    private vendorQuotationService: VendorQuotationService,
    private purchaseRequisitionService: PurchaseRequisitionService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getVendorQuotations("");
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      requisitionNo: [""],
    });
  }

  getPurchaseRequisitions(keyword): void {
    let purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();
    purchaseRequisitionRequest.requisitionStatusIds = [6];
    purchaseRequisitionRequest.keyword = keyword;
    this.purchaseRequisitionService
      .getPurchaseRequisitions(purchaseRequisitionRequest)
      .subscribe((res) => {
        console.log(res);
        this.purchaseRequisitions = res?.data?.item1;
      });
  }

  getVendorQuotations(requisitionNo): void {
    this.vendorQuotationRequest.vendorQuotationStatus = 2;
    this.vendorQuotationRequest.requisitionNo = requisitionNo;
    this.vendorQuotationService
      .getVendorQuotations(this.vendorQuotationRequest)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<VendorQuotationResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  onRequisitionNoChange(): void {
    const term = this.searchForm.get("requisitionNo");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.getPurchaseRequisitions(filterValue);
  }

  onSearch() {
    this.loading = true;
    let requisitionNo = this.searchForm.get("requisitionNo").value;
    this.getVendorQuotations(requisitionNo);
    console.log(this.searchForm.value);
  }

  onPageChange(pageEvent) {
    this.vendorQuotationRequest.page = pageEvent.pageIndex;
    this.vendorQuotationRequest.rowsPerPage = pageEvent.pageSize;
    this.getVendorQuotations("");
  }
  handleRadioChange($event) {
    this.selectedVendorQuotations = $event.value;
  }
}
