import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";
import { PostType } from "app/shared/enums/postType";

@Component({
    selector: "app-lc-adjustment-detail",
    templateUrl: "./lc-adjustment-detail.component.html",
    styleUrls: ["./lc-adjustment-detail.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class LcAdjustmentDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "accountId",
    "accountDescription",
    "postType",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }

  getPostTypeName(value: number) {
    return PostType[value];
  }
}
