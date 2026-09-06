import { Component, Input, OnInit } from "@angular/core";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";

@Component({
  selector: "app-purchase-invoice-list-detail",
  templateUrl: "./purchase-invoice-list-detail.component.html",
  styleUrls: ["./purchase-invoice-list-detail.component.scss"],
})
export class PurchaseInvoiceListDetailComponent implements OnInit {
  /**
   *
   */
  constructor(public dateFormatService:DateTimeFormatService) {
    
  }
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "grnno",
    "grndate",
    "productId",
    "measurementUnitId",
    "grnquantity",
    "rate",
    "amount"
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
