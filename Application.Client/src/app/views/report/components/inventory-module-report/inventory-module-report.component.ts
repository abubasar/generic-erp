import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-inventory-module-report",
  templateUrl: "./inventory-module-report.component.html",
  styleUrls: ["./inventory-module-report.component.scss"],
})
export class InventoryModuleReportComponent implements OnInit {
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
