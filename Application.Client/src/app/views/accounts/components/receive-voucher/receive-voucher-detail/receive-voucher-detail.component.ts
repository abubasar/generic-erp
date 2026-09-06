import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-receive-voucher-detail",
  templateUrl: "./receive-voucher-detail.component.html",
  styleUrls: ["./receive-voucher-detail.component.scss"],
})
export class ReceiveVoucherDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "accountId",
    "accountDescription",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
