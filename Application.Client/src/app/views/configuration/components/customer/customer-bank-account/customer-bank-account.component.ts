import { Component, Input, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-customer-bank-account",
    templateUrl: "./customer-bank-account.component.html",
    styleUrls: ["./customer-bank-account.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class CustomerBankAccountComponent implements OnInit {
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
