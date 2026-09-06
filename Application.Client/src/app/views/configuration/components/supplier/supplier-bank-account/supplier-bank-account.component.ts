import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-supplier-bank-account",
  templateUrl: "./supplier-bank-account.component.html",
  styleUrls: ["./supplier-bank-account.component.scss"],
})
export class SupplierBankAccountComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "name",
    "accNo",
    "routingNo",
    "bankName",
    "branchName",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }
}
