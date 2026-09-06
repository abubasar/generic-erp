import { Component, Input, OnInit } from "@angular/core";
import { PostType } from "app/shared/enums/postType";

@Component({
  selector: "app-journal-entry-detail",
  templateUrl: "./journal-entry-detail.component.html",
  styleUrls: ["./journal-entry-detail.component.scss"],
})
export class JournalEntryDetailComponent implements OnInit {
  @Input() data: any[] = [];
  displayedColumns: string[] = [
    "sl",
    "accountId",
    //"accountDescription",
    "postType",
    //"isOpeningBalance",
    "amount",
  ];
  dataSource: any[] = [];
  ngOnInit() {
    this.dataSource = this.data;
    console.log("Data Source:", this.dataSource);
  }

  getPostTypeName(value: number) {
    return PostType[value];
  }
}
