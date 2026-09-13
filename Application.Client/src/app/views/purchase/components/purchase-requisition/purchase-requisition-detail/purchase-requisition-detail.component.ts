import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-purchase-requisition-detail",
    templateUrl: "./purchase-requisition-detail.component.html",
    styleUrls: ["./purchase-requisition-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PurchaseRequisitionDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "quantity",
    // "rate",
    // "amount",
    "lastPurchaseRate",
    "orderedQuantity",
    "alertQuantity",
    "currentStockQuantity",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
