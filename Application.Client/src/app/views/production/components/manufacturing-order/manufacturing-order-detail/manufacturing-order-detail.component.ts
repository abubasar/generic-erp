import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-manufacturing-order-detail",
    templateUrl: "./manufacturing-order-detail.component.html",
    styleUrls: ["./manufacturing-order-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ManufacturingOrderDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "rawMaterialId",
    "measurementUnitId",
    "quantity",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
