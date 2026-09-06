import { Component, Input, OnInit } from '@angular/core';
import { DateTimeFormatService } from 'app/shared/services/date-time-format.service';

@Component({
  selector: "app-purchase-invoice-detail",
  templateUrl: "./purchase-invoice-detail.component.html",
  styleUrls: ["./purchase-invoice-detail.component.scss"],
})
export class PurchaseInvoiceDetailComponent implements OnInit {
  constructor(public dataFormatService: DateTimeFormatService) {}
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "grnno",
    "grndate",
    "grnquantity",
    "rate",
    "amount",
    "vatPercentage",
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
