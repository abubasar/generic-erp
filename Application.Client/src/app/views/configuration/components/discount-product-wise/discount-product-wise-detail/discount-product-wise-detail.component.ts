import { Component, Input, OnInit } from "@angular/core";
import { DiscountProductWiseDetail } from "app/views/configuration/models/discount-product-wise/discount-product-wise.model";

@Component({
  selector: "app-discount-product-wise-detail",
  templateUrl: "./discount-product-wise-detail.component.html",
  styleUrls: ["./discount-product-wise-detail.component.scss"],
})
export class DiscountProductWiseDetailComponent implements OnInit {
  @Input() data: DiscountProductWiseDetail[] = [];
  displayedColumns: string[] = ["sl", "productId", "discountAmountPerKg"];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    //console.log("Data Source:", this.dataSource);
  }
}
