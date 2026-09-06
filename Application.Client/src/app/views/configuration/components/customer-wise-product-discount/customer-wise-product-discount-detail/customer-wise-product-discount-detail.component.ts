import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-customer-wise-product-discount-detail",
  templateUrl: "./customer-wise-product-discount-detail.component.html",
  styleUrls: ["./customer-wise-product-discount-detail.component.scss"],
})
export class CustomerWiseProductDiscountDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "salePrice",
    "invoiceDiscount",
    "cashDiscount",
    "specialDiscount",
    "monthlyDiscount",
    "yearlyDiscount",
    "targetDiscount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    // console.log("Data Source:", this.dataSource);
  }
}
