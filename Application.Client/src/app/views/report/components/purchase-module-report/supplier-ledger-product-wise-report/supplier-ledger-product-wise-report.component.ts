import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-supplier-ledger-product-wise-report",
  templateUrl: "./supplier-ledger-product-wise-report.component.html",
  styleUrls: ["./supplier-ledger-product-wise-report.component.scss"],
})
export class SupplierLedgerProductWiseReportComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  costCenters: CostCenter[];
  suppliers: Supplier[];
  filterSuppliers: Supplier[];

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private costCenterService: CostCenterService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.getAllSuppliers();
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
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data.item1;
    });
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
      this.filterSuppliers = res.data?.item1;
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "accountId") {
      this.searchForm?.get("accountId").setValue(null);
    }
  }

  print($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.isLoading2 = true;
      this.searchForm.get("reportType").setValue(2);
    }
    this.http
      .post(
        environment.apiURL + "/Account/supplier-ledger-product-wise-print",
        this.searchForm.value,
        { responseType: "blob" }
      )
      .subscribe((response) => {
        if (response.type === "text/plain") {
          window.alert("No Result Found");
          return;
        }
        if ($event?.submitter?.name == "pdf") {
          //Create a Blob from the PDF Stream
          const file = new Blob([response], { type: "application/pdf" });
          //Build a URL from the file
          const fileURL = URL.createObjectURL(file);
          //Open the URL on new Window
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
          link.setAttribute("download", "supplier_ledger_product_wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
