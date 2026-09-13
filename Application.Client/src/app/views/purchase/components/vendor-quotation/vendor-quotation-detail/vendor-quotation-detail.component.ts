import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-vendor-quotation-detail",
    templateUrl: "./vendor-quotation-detail.component.html",
    styleUrls: ["./vendor-quotation-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class VendorQuotationDetailComponent implements OnInit {
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
