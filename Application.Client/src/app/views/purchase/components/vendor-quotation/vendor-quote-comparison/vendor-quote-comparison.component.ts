import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { Router } from "@angular/router";
import { VendorQuotationStatus } from "app/shared/enums/vendorQuotationStatus";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { LocalStoreService } from "app/shared/services/local-store.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { VendorQuotationResponseDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-response-dto.model";
import { VendorQuotationSearchRequestDTO } from "app/views/purchase/models/vendor-quotation/vendor-quotation-search-request-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";
import { VendorQuotationService } from "app/views/purchase/services/vendor-quotation.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-vendor-quote-comparison",
  templateUrl: "./vendor-quote-comparison.component.html",
  styleUrls: ["./vendor-quote-comparison.component.scss"],
})
export class VendorQuoteComparisonComponent implements OnInit {
  statusCode: number;
  isLoading: boolean = false;
  panelOpenState: boolean;
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  purchaseRequisitions: PurchaseRequisitionResponseDTO[];

  private path = {
    view: "purchase/manage-vendor-quotation/view",
  };

  displayedColumns: string[] = [
    "sl",
    "productId",
    "quantity",
    "rate",
    "amount",
  ];
  dataSource: VendorQuotationResponseDTO[];
  totalCount: number;
  vendorQuotationRequest = new VendorQuotationSearchRequestDTO();
  purchaseRequisitionRequest = new PurchaseRequisitionSearchRequestDTO();

  constructor(
    private vendorQuotationService: VendorQuotationService,
    public statusColorService: StatusColorService,
    private purchaseRequisitionService: PurchaseRequisitionService,
    public dateFormatService: DateTimeFormatService,
    private fb: FormBuilder,
    private router: Router,
    private localStorageService: LocalStoreService,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      requisitionNo: [""],
    });
  }

  onRequisitionNoChange(): void {
    const term = this.searchForm
      .get("requisitionNo")
      .value.toLowerCase()
      .trim();
    this.getPurchaseRequisitions(term);
  }

  getTotal(arr) {
    return arr.reduce((accumulator, object) => {
      return accumulator + object.amount;
    }, 0);
  }
  getPurchaseRequisitions(requisitionNo: string): void {
    //! requisitionStatus = 5 for RFQSent //
    this.purchaseRequisitionRequest.requisitionStatusIds = [3, 5];
    this.purchaseRequisitionRequest.keyword = requisitionNo;
    this.purchaseRequisitionService
      .getPurchaseRequisitions(this.purchaseRequisitionRequest)
      .subscribe((res) => {
        console.log(res);
        this.purchaseRequisitions = res?.data?.item1;
      });
  }

  getAllVendorQuotations(): void {
    const requisitionNo = this.searchForm
      .get("requisitionNo")
      .value.toLowerCase()
      .trim();
    if (requisitionNo) {
      this.vendorQuotationRequest.requisitionNo = requisitionNo;
      this.vendorQuotationService
        .getVendorQuotations(this.vendorQuotationRequest)
        .subscribe((res) => {
          this.statusCode = res.statusCode;
          this.dataSource = res?.data?.item1;
          console.log(res);
          console.log(this.dataSource);
          this.isLoading = false;
        });
    }
  }

  getVendorQuotationStatus(value) {
    return this.statusColorService.getVendorQuotationStatus(value);
  }

  getVendorQuotationStatusName(value: number) {
    return VendorQuotationStatus[value];
  }

  // navigateToView(item) {
  //   this.router.navigate([this.path.view, item.id]);
  //   this.localStorageService.setItem(item.id, item);
  // }

  approve(id: string, requisitionNo: string) {
    this.isLoading = true;
    this.vendorQuotationService
      .approveVendorQuotation(id, requisitionNo)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getAllVendorQuotations();
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  unpost(requisitionNo: string) {
    this.isLoading = true;
    console.log(requisitionNo);
    this.vendorQuotationService
      .unpostVendorQuotation(requisitionNo)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getAllVendorQuotations();
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  onSearch() {
    this.isLoading = true;
    this.getAllVendorQuotations();
  }
}
