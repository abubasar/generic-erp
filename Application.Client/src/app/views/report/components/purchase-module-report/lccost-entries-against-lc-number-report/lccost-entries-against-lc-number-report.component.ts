import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-lccost-entries-against-lc-number-report",
  templateUrl: "./lccost-entries-against-lc-number-report.component.html",
  styleUrls: ["./lccost-entries-against-lc-number-report.component.scss"],
})
export class LccostEntriesAgainstLcNumberReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private purchaseOrderService: PurchaseOrderService
  ) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();
  purchaseOrders: PurchaseOrderResponseDTO[];
  filterPurchaseOrders: PurchaseOrderResponseDTO[];

  ngOnInit(): void {
    this.initializeForm();
    this.getPurchaseOrders();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      //fromDate: ["", Validators.required],
      //toDate: ["", Validators.required],
      purchaseOrderId: [null],
      reportType: [1, Validators.required],
    });
  }

  getPurchaseOrders(): void {
    this.purchaseOrderRequest.isImportPurchase = true;
    this.purchaseOrderRequest.purchaseOrderStatuses = [3, 4, 5, 6, 7, 8];
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        this.purchaseOrders = res?.data?.item1;
      });
  }

  handlePurchaseOrderSearch(event: any): void {
    const name = event.target?.name;
    if (name === "purchaseOrderId") {
      const term = this.searchForm.get("purchaseOrderId");
      this.filterPurchaseOrder(term.value || "");
    }
  }

  private filterPurchaseOrder(value: string) {
    const filterValue = value.toLowerCase();
    this.filterPurchaseOrders = this.purchaseOrders?.filter((option) =>
      option.lcNumber.toLowerCase().includes(filterValue)
    );
  }

  getPurchaseOrderNoOrLcNumber(purchaseOrderId: string) {
    if (!purchaseOrderId) {
      return;
    }
    const purchaseOrderAccount = this.purchaseOrders?.find(
      (purchaseOrder) => purchaseOrder?.id === purchaseOrderId
    );
    return ("LC No: "+
      purchaseOrderAccount?.lcNumber +
      " (" +
      purchaseOrderAccount?.ponumber +
      ")"
    );
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "purchaseOrderId") {
      this.searchForm?.get("purchaseOrderId").setValue(null);
    }
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.isLoading2 = true;
      this.searchForm.get("reportType").setValue(2);
    }
    console.log(this.searchForm.value);

    this.http
      .get(
        environment.apiURL +
          "/LCCostEntry/lc-cost-entry-against-po-print/" +
          this.searchForm.get("purchaseOrderId").value +
          "/" +
          this.searchForm.get("reportType").value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
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
          link.setAttribute(
            "download",
            "lc_cost_entries_against_lc_number.xlsx"
          );
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
