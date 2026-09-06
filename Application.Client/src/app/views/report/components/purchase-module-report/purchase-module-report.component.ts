import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-purchase-module-report",
  templateUrl: "./purchase-module-report.component.html",
  styleUrls: ["./purchase-module-report.component.scss"],
})
export class PurchaseModuleReportComponent implements OnInit {
  selectedForm: string = "";
  constructor() {}

  ngOnInit(): void {}
  loadForm(formName: string) {
    this.selectedForm = formName;
  }

  getButtonClasses(formName: string): any {
    return {
      "mat-raised-button": this.selectedForm === formName,
      "mat-stroked-button": this.selectedForm !== formName,
      "mat-primary": this.selectedForm === formName,
    };
  }
}
