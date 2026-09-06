import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: "app-money-receipt-detail",
  templateUrl: "./money-receipt-detail.component.html",
  styleUrls: ["./money-receipt-detail.component.scss"],
})
export class MoneyReceiptDetailComponent implements OnInit {
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
