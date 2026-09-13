import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-sale-return-detail",
    templateUrl: "./sale-return-detail.component.html",
    styleUrls: ["./sale-return-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SaleReturnDetailComponent implements OnInit {
  @Input() data: any[] = [];
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
      "returnQuantity",
      "returnBonusQuantity",
    ], // Columns to hide for Pharmacy
    "2": ["discountPercentage", "percentageDiscountAmount"], // Columns to hide for Feed
    // Add more configurations as needed
  };

  updateDisplayedColumns() {
    const key = this.industryProfile.isFeed ? "2" : "1";
    const columnsToHide = this.columnConfig[key] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }
}
