import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-payment-voucher-detail',
  templateUrl: './payment-voucher-detail.component.html',
  styleUrls: ['./payment-voucher-detail.component.scss']
})
export class PaymentVoucherDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = ["sl", "accountId", "accountDescription", "amount"];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
