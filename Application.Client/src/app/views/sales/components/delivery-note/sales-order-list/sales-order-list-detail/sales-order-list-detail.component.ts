import { Component, Input, OnInit } from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";

@Component({
  selector: "app-sales-order-list-detail",
  templateUrl: "./sales-order-list-detail.component.html",
  styleUrls: ["./sales-order-list-detail.component.scss"],
})
export class SalesOrderListDetailComponent implements OnInit {
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
    "netRate",
    "discountPerUnit",
    "specialDiscountPerUnit",
    "discountAmount",
    "amount",
  ];
  dataSource: any[] = [];

  constructor(private jwtAuth: JwtAuthService) {}

  ngOnInit() {
    this.dataSource = this.data;
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.updateDisplayedColumns();
  }

  private columnConfig = {
    "1": ["bagWeight", "quantity", "bonusQuantity"], // Columns to hide for Pharmacy
    "2": [], // Show all columns for Feed
    // Add more configurations as needed
  };

  updateDisplayedColumns() {
    const columnsToHide = this.columnConfig[this.businessType] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }
}
