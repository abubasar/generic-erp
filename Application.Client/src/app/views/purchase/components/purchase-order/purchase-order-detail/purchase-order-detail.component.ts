import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-purchase-order-detail",
  templateUrl: "./purchase-order-detail.component.html",
  styleUrls: ["./purchase-order-detail.component.scss"],
})
export class PurchaseOrderDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "quantity",
    "receivedQuantity",
    "rate",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    this.updateDisplayedColumns();
  }
  updateDisplayedColumns() {
    if (this.data.some((item) => item.currencyRate > 0)) {
      this.displayedColumns.push("currencyRate");
      this.displayedColumns.push("currencyAmount");
    }
  }
}
