import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-purchase-requisition-detail",
  templateUrl: "./purchase-requisition-detail.component.html",
  styleUrls: ["./purchase-requisition-detail.component.scss"],
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
