import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-supplier-payment-detail',
  templateUrl: './supplier-payment-detail.component.html',
  styleUrls: ['./supplier-payment-detail.component.scss']
})
export class SupplierPaymentDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = ["sl", "accountId", "accountDescription", "purchaseInvoiceNo", "amount"];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
