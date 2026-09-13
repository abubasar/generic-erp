import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ChangeDetectionStrategy
} from "@angular/core";
import { IndustryProfileService } from "app/shared/services/industry-profile/industry-profile.service";

@Component({
    selector: "app-stock-transfer-detail",
    templateUrl: "./stock-transfer-detail.component.html",
    styleUrls: ["./stock-transfer-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class StockTransferDetailComponent implements OnInit, OnChanges {
  @Input() data: any[] = [];
  @Input() isFinishedGoodsInventoryType: boolean = false;
  isFinishedGoods: boolean = false;
  displayedColumns: string[] = [
    "sl",
    "productId",
    "measurementUnitId",
    "bagWeight",
    "transferBagQuantity",
    "transferQuantity",
    "currentStockQuantity",
  ];
  dataSource: any[] = [];
  constructor(private industryProfile: IndustryProfileService) {}
  ngOnInit() {
    this.isFinishedGoods = this.isFinishedGoodsInventoryType;
    this.dataSource = this.data;
    this.updateDisplayedColumns();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.isFinishedGoodsInventoryType) {
      this.isFinishedGoods = changes.isFinishedGoodsInventoryType.currentValue;
      this.updateDisplayedColumns();
    }
  }

  updateDisplayedColumns() {
    // Pharma never shows bag columns; Feed shows them only for finished goods.
    const columnsToHide =
      this.industryProfile.isFeed && this.isFinishedGoods
        ? []
        : ["bagWeight", "transferBagQuantity"];
    this.displayedColumns = [
      "sl",
      "productId",
      "measurementUnitId",
      "bagWeight",
      "transferBagQuantity",
      "transferQuantity",
      "currentStockQuantity",
    ].filter((col) => !columnsToHide.includes(col));
  }
}
