import { Component, Input, OnInit } from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";

@Component({
  selector: "app-sale-return-detail",
  templateUrl: "./sale-return-detail.component.html",
  styleUrls: ["./sale-return-detail.component.scss"],
})
export class SaleReturnDetailComponent implements OnInit {
  @Input() data: any[] = [];
  businessType: string;
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "bagWeight",
    "primaryQuantity",
    "quantity",
    "primaryBonusQuantity",
    "bonusQuantity",
    "rate",
    "returnPrimaryQuantity",
    "returnQuantity",
    "returnPrimaryBonusQuantity",
    "returnBonusQuantity",
    "discountPercentage",
    "percentageDiscountAmount",
    "amount",
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
      "returnQuantity",
      "returnBonusQuantity",
    ], // Columns to hide for Pharmacy
    "2": ["discountPercentage", "percentageDiscountAmount"], // Columns to hide for Feed
    // Add more configurations as needed
  };

  updateDisplayedColumns() {
    const columnsToHide = this.columnConfig[this.businessType] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }
}
