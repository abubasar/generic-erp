import { Component, Input, OnInit } from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";

@Component({
  selector: "app-sale-invoice-detail",
  templateUrl: "./sale-invoice-detail.component.html",
  styleUrls: ["./sale-invoice-detail.component.scss"],
})
export class SaleInvoiceDetailComponent implements OnInit {
  @Input() data: any[] = [];
  businessType: string;
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "primaryQuantity",
    "quantity",
    "primaryBonusQuantity",
    "bonusQuantity",
    "rate",
    "discountPercentage",
    "percentageDiscountAmount",
    "netRate",
    "discountPerUnit",
    "offerDiscountPerUnit",
    "discountAmount",
    "amount",
    "deliveryDate",
    "deliveryPlace",
  ];

  dataSource: any[] = [];

  constructor(
    public dateFormatService: DateTimeFormatService,
    private jwtAuth: JwtAuthService
  ) {}

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
      "quantity",
      "bonusQuantity",
      "netRate",
      "discountPerUnit",
      "offerDiscountPerUnit",
      "discountAmount",
    ], // Columns to hide for Pharmacy
    "2": ["discountPercentage", "percentageDiscountAmount"], // Show all columns for Feed
    // Add more configurations as needed
  };

  updateDisplayedColumns() {
    const columnsToHide = this.columnConfig[this.businessType] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }
}
