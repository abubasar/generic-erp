import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-manufacturing-order-detail",
  templateUrl: "./manufacturing-order-detail.component.html",
  styleUrls: ["./manufacturing-order-detail.component.scss"],
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
