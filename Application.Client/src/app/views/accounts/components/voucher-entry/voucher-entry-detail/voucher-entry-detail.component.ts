import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-voucher-entry-detail",
  templateUrl: "./voucher-entry-detail.component.html",
  styleUrls: ["./voucher-entry-detail.component.scss"],
})
export class VoucherEntryDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = ["sl", "accountId", "accountDescription", "amount"];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
