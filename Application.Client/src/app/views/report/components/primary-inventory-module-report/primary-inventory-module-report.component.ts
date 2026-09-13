import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-primary-inventory-module-report",
    templateUrl: "./primary-inventory-module-report.component.html",
    styleUrls: ["./primary-inventory-module-report.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class PrimaryInventoryModuleReportComponent implements OnInit {
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
