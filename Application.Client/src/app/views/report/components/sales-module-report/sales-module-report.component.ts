import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-sales-module-report",
    templateUrl: "./sales-module-report.component.html",
    styleUrls: ["./sales-module-report.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class SalesModuleReportComponent implements OnInit {
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
