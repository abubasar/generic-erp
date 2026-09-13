import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-sale-invoice-list-detail",
    templateUrl: "./sale-invoice-list-detail.component.html",
    styleUrls: ["./sale-invoice-list-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SaleInvoiceListDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "quantity",
    "rate",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
