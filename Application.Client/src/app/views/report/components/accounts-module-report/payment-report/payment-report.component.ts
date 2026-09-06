import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { PaymentReportFilterType } from "app/shared/enums/paymentReportFilterType";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { PaymentModeService } from "app/views/configuration/services/payment-mode.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-payment-report",
  templateUrl: "./payment-report.component.html",
  styleUrls: ["./payment-report.component.scss"],
})
export class PaymentReportComponent implements OnInit {
  paymentReportFilterTypes = Object.keys(PaymentReportFilterType).map(
    (key) => ({
      name: key,
      value: PaymentReportFilterType[key],
    })
  );
  paymentModes: PaymentMode[];
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private costCenterService: CostCenterService,
    private paymentModeService: PaymentModeService
  ) {}
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  searchForm: FormGroup;
  costCenters: CostCenter[];
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  showFilterBySupplier: boolean = false;
  ngOnInit(): void {
    this.initializeForm();
    this.getAllSuppliers();
    this.getAllCostCenters();
    this.getAllPaymentModes();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      costCenterId: [null],
      paymentModeId: [null],
      accountTransactionType: [null],
      accountId: [null],
      reportType: [1, Validators.required],
    });
  }
  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data.item1;
    });
  }
  getAllPaymentModes() {
    this.paymentModeService.getAllPaymentModes().subscribe((res) => {
      this.paymentModes = res.data?.item1;
    });
  }
  onSelectedFilterType(accountTransactionType) {
    if (accountTransactionType === PaymentReportFilterType.Supplier_Payment) {
      this.showFilterBySupplier = true;
    } else {
      this.showFilterBySupplier = false;
      this.searchForm.get("accountId").reset();
    }
  }
  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.searchForm.get("accountId");
      this.filterSupplier(term.value || "");
    }
  }

  private filterSupplier(value: string) {
    const filterValue = value.toLowerCase();
    this.filterSuppliers = this.suppliers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.suppliers?.find(
      (supplier) => supplier?.id === supplierId
    );
    return supplierAccount?.name;
  }
  getAllSuppliers() {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.suppliers = res.data?.item1;
    });
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    this.http
      .post(
        environment.apiURL + "/VoucherEntry/payment-report/print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
          const file = new Blob([response], { type: "application/pdf" });
          const fileURL = URL.createObjectURL(file);
          const pdfWindow = window.open();
          pdfWindow.location.href = fileURL;
          this.isLoading1 = false;
        } else {
          const blob = new Blob([response], {
            type: "application/octet-stream",
          });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = url;
          link.setAttribute("download", "payment_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
