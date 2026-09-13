import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-lc-cost-entry-detail",
    templateUrl: "./lc-cost-entry-detail.component.html",
    styleUrls: ["./lc-cost-entry-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class LcCostEntryDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "debitAccountId",
    "creditAccountId",
    "amount",
    "isIncludedWithinLandedCost",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
