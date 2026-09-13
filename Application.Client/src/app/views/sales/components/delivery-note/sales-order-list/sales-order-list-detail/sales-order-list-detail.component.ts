import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-sales-order-list-detail",
    templateUrl: "./sales-order-list-detail.component.html",
    styleUrls: ["./sales-order-list-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SalesOrderListDetailComponent implements OnInit {
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
    "netRate",
    "discountPerUnit",
    "specialDiscountPerUnit",
    "discountAmount",
    "amount",
  ];
  dataSource: any[] = [];

  constructor(public industryProfile: IndustryProfileService) {}

  ngOnInit() {
    this.dataSource = this.data;
    this.updateDisplayedColumns();
  }

  private columnConfig = {
    "1": ["bagWeight", "quantity", "bonusQuantity"], // Columns to hide for Pharmacy
    "2": [], // Show all columns for Feed
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
