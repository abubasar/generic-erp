import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-production-module-report",
  templateUrl: "./production-module-report.component.html",
  styleUrls: ["./production-module-report.component.scss"],
})
export class ProductionModuleReportComponent implements OnInit {
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
