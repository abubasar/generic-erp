import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-receive-payment-detail',
  templateUrl: './receive-payment-detail.component.html',
  styleUrls: ['./receive-payment-detail.component.scss']
})
export class ReceivePaymentDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = ["sl", "accountId", "accountDescription", "amount"];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
