import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-customer-ledger-product-wise-report",
  templateUrl: "./customer-ledger-product-wise-report.component.html",
  styleUrls: ["./customer-ledger-product-wise-report.component.scss"],
})
export class CustomerLedgerProductWiseReportComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  costCenters: CostCenter[];
  customers: Customer[];
  filterCustomers: Customer[];
  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private costCenterService: CostCenterService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.getAllCustomers();
    this.getAllCostCenters();
    this.initializeForm();
  }
  private initializeForm() {
    this.searchForm = this.fb.group({
      accountId: [null, Validators.required],
      costCenterId: [null],
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.accountId === "") {
        this.searchForm
          .get("accountId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("accountId").setValue(null);
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data.item1;
    });
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.searchForm.get("accountId");
      this.filterCustomer(term.value || "");
    }
  }

  private filterCustomer(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterCustomers = this.customers?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerAccount?.name;
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.filterCustomers = this.customers = res.data?.item1;
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
        environment.apiURL + "/Account/Customer-ledger-product-wise-print",
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
          link.setAttribute("download", "Customer_Ledger_Product_Wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
