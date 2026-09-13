import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-stock-adjustment-detail",
    templateUrl: "./stock-adjustment-detail.component.html",
    styleUrls: ["./stock-adjustment-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
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
