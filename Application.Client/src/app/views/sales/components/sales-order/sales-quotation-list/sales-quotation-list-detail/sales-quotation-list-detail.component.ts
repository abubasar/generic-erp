import { Component, Input, OnInit } from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";

@Component({
  selector: "app-sales-quotation-list-detail",
  templateUrl: "./sales-quotation-list-detail.component.html",
  styleUrls: ["./sales-quotation-list-detail.component.scss"],
})
export class SalesQuotationListDetailComponent implements OnInit {
  @Input() data: any[] = [];
  businessType: string;
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "bagWeight",
    "primaryQuantity",
    "quantity",
    "rate",
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
    "1": ["bagWeight", "quantity"], // Columns to hide for Pharmacy
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
