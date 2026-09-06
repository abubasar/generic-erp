import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from "@angular/core";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";

@Component({
  selector: "app-stock-transfer-detail",
  templateUrl: "./stock-transfer-detail.component.html",
  styleUrls: ["./stock-transfer-detail.component.scss"],
})
export class StockTransferDetailComponent implements OnInit, OnChanges {
  @Input() data: any[] = [];
  @Input() isFinishedGoodsInventoryType: boolean = false;
  isFinishedGoods: boolean = false;
  businessType: string = "";
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
  constructor(private jwtAuth: JwtAuthService) {}
  ngOnInit() {
    this.isFinishedGoods = this.isFinishedGoodsInventoryType;
    this.dataSource = this.data;
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
      // Conditionally set columns based on businessType
      this.updateDisplayedColumns();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.isFinishedGoodsInventoryType) {
      this.isFinishedGoods = changes.isFinishedGoodsInventoryType.currentValue;
      this.updateDisplayedColumns();
    }
  }

  updateDisplayedColumns() {
    const columnConfig = {
      "1": ["bagWeight", "transferBagQuantity"], // Columns to hide for Pharmacy
      "2": this.isFinishedGoods ? [] : ["bagWeight", "transferBagQuantity"], // Show all columns for Feed
    };
    const columnsToHide = columnConfig[this.businessType] || [];
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
