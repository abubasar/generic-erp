import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-purchase-return-detail",
  templateUrl: "./purchase-return-detail.component.html",
  styleUrls: ["./purchase-return-detail.component.scss"],
})
export class PurchaseReturnDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "grnquantity",
    "grnBagWeightDeductionQty",
    "quantity",
    "numberOfBagQuantity",
    "bagWeightDeductionQty",
    "netQuantity",
    "rate",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
