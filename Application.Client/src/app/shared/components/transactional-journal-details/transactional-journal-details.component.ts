import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: "app-transactional-journal-details",
  templateUrl: "./transactional-journal-details.component.html",
  styleUrls: ["./transactional-journal-details.component.scss"],
})
export class TransactionalJournalDetailsComponent implements OnInit {
  displayedColumns: string[] = ["account", "debit", "credit"];

  @Input() test: any[] = [];
  @Input() accounts: any[] = [];
  @Input() values: any[] = [];

  dataSource: any[] = [];

  ngOnInit() {
    this.dataSource = this.accounts;
  }
}

// dataSource = [
//   { accountName: "Purchase Account", column: "debit", value: 1000 },
//   { accountName: "Supplier Account", column: "credit", value: 1000 },
//   { accountName: "", row: "total", value: 1000},
// ];

// accounts = [
//   { id: 1, accountName: "Purchase Account", column: "credit" },
//   { id: 2, accountName: "Freight Or Additional Cost", column: "credit" },
//   { id: 3, accountName: "RM Inventory Account", column: "debit" },
//   { id: 4, accountName: "", row: "total" },
// ];

// values = [
//   {
//     id: 1,
//     value: data?.subtotal,
//   },
//   {
//     id: 2,
//     value: data?.freightOrAdditionalCost,
//   },
//   { id: 3, value: data?.total },
//   { id: 4, value: data?.total },
// ];

// this.accounts = this.accounts.map((acc) => ({
//   ...acc,
//   value: values.find((val) => val.id === acc.id)?.value,
// }));
