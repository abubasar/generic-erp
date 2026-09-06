import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-stock-adjustment-detail",
  templateUrl: "./stock-adjustment-detail.component.html",
  styleUrls: ["./stock-adjustment-detail.component.scss"],
})
export class StockAdjustmentDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "adjustmentQty",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
