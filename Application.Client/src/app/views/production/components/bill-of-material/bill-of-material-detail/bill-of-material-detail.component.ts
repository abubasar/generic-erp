import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-bill-of-material-detail",
  templateUrl: "./bill-of-material-detail.component.html",
  styleUrls: ["./bill-of-material-detail.component.scss"],
})
export class BillOfMaterialDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "rawMaterialId",
    "measurementUnitId",
    "quantity",
    "percentage",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
