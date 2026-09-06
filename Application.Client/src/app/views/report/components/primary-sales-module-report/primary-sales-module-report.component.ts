import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-primary-sales-module-report",
  templateUrl: "./primary-sales-module-report.component.html",
  styleUrls: ["./primary-sales-module-report.component.scss"],
})
export class PrimarySalesModuleReportComponent implements OnInit {
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
