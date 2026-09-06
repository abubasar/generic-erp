import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-supplier-payment-dialog-detail",
  templateUrl: "./supplier-payment-dialog-detail.component.html",
  styleUrls: ["./supplier-payment-dialog-detail.component.scss"],
})
export class SupplierPaymentDialogDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "accountId",
    "accountDescription",
    "purchaseInvoiceNo",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
  }
}
