import { Component, Input, OnInit } from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";

@Component({
  selector: "app-sales-order-detail",
  templateUrl: "./sales-order-detail.component.html",
  styleUrls: ["./sales-order-detail.component.scss"],
})
export class SalesOrderDetailComponent implements OnInit {
  @Input() data: any[] = [];
  businessType: string;
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

  constructor(private jwtAuth: JwtAuthService) {}

  ngOnInit() {
    this.dataSource = this.data;
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    // Conditionally set columns based on businessType
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
    const columnsToHide = this.columnConfig[this.businessType] || [];
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
