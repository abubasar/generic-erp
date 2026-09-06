import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-goods-receive-note-detail",
  templateUrl: "./goods-receive-note-detail.component.html",
  styleUrls: ["./goods-receive-note-detail.component.scss"],
})
export class GoodsReceiveNoteDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "poquantity",
    "batchNo",
    "expiryDate",
    "grnquantity",
    "bagWeightDeductionQuantity",
    "numberOfBagQuantity",
    "netQuantity",
    // "rejectedQuantity",
    // "rejectionReason",
    "rate",
    "rateAfterBagWeightDeduction",
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
