import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: "app-po-price-adjustment-after-grn-detail",
    templateUrl: "./po-price-adjustment-after-grn-detail.component.html",
    styleUrls: ["./po-price-adjustment-after-grn-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PoPriceAdjustmentAfterGrnDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "grnQuantity",
    "grnRate",
    "adjustmentQuantity",
    "adjustmentRate",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
