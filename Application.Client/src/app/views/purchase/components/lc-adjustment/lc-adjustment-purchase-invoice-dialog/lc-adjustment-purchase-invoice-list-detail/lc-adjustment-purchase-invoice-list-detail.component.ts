import { Component, Input, OnInit } from "@angular/core";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";

@Component({
  selector: "app-lc-adjustment-purchase-invoice-list-detail",
  templateUrl: "./lc-adjustment-purchase-invoice-list-detail.component.html",
  styleUrls: ["./lc-adjustment-purchase-invoice-list-detail.component.scss"],
})
export class LcAdjustmentPurchaseInvoiceListDetailComponent implements OnInit {
  constructor(public dateFormatService: DateTimeFormatService) {}
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "grnno",
    "grndate",
    "productId",
    "measurementUnitId",
    "grnquantity",
    "rate",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
