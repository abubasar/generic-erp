import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-sale-invoice-detail",
    templateUrl: "./sale-invoice-detail.component.html",
    styleUrls: ["./sale-invoice-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SaleInvoiceDetailComponent implements OnInit {
  @Input() data: any[] = [];
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
    public industryProfile: IndustryProfileService
  ) {}

  ngOnInit() {
    this.dataSource = this.data;
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
    const key = this.industryProfile.isFeed ? "2" : "1";
    const columnsToHide = this.columnConfig[key] || [];
    this.displayedColumns = this.displayedColumns.filter(
      (col) => !columnsToHide.includes(col)
    );
  }
}
