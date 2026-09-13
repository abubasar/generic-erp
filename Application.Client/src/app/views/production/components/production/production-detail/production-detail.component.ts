import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-production-detail",
    templateUrl: "./production-detail.component.html",
    styleUrls: ["./production-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ProductionDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "rawMaterialId",
    "measurementUnitId",
    "quantity",
    "adjustmentQuantity",
    "actualUsedQuantity",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
