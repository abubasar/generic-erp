import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-delivery-note-detail",
    templateUrl: "./delivery-note-detail.component.html",
    styleUrls: ["./delivery-note-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class DeliveryNoteDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "productId",
    "orderedPrimaryQuantity",
    "orderedQuantity",
    "deliveryPrimaryQuantity",
    "deliveryQuantity",
    "orderedPrimaryBonusQuantity",
    "orderedBonusQuantity",
    "deliveryPrimaryBonusQuantity",
  ];
  dataSource: any[] = [];

  constructor(public industryProfile: IndustryProfileService) {}

  ngOnInit() {
    this.dataSource = this.data;
    this.updateDisplayedColumns();
  }

  private columnConfig = {
    "1": ["orderedQuantity", "deliveryQuantity", "orderedBonusQuantity"], // Columns to hide for Pharmacy
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
