import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-sales-order-detail",
    templateUrl: "./sales-order-detail.component.html",
    styleUrls: ["./sales-order-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SalesOrderDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "bagWeight",
    "primaryQuantity",
    "primaryBonusQuantity",
    "quantity",
    "bonusQuantity",
    "rate",
    "discountPercentage",
    "percentageDiscountAmount",
    "discountPerUnit",
    "offerDiscountPerUnit",
    "netRate",
    "amount",
    "deliveredPrimaryQuantity",
    "deliveredPrimaryBonusQuantity",
  ];
  dataSource: any[] = [];

  constructor(public industryProfile: IndustryProfileService) {}

  ngOnInit() {
    this.dataSource = this.data;
    this.updateDisplayedColumns();
  }

  private columnConfig = {
    "1": [
      "bagWeight",
      "quantity",
      "bonusQuantity",
      "discountPerUnit",
      "offerDiscountPerUnit",
      "netRate",
    ], // Columns to hide for Pharmacy
    "2": ["discountPercentage", "percentageDiscountAmount"], // Hidden columns for Feed
    // Add more configurations as needed
  };

  updateDisplayedColumns() {
    const key = this.industryProfile.isFeed ? "2" : "1";
    const columnsToHide = this.columnConfig[key] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }

  // updateDisplayedColumns() {
  //   if (this.businessType === "1") {
  //     // Example: Pharmacy business
  //     this.displayedColumns = this.displayedColumns.filter((col) => {
  //       return (
  //         col !== "bagWeight" && col !== "quantity" && col !== "bonusQuantity"
  //       ); // Hide these columns
  //     });
  //   } else if (this.businessType === "2") {
  //     // Example: Feed business
  //     this.displayedColumns = this.displayedColumns.filter((col) => {
  //       return true; //show all columns
  //     });
  //   }
  // }
}
