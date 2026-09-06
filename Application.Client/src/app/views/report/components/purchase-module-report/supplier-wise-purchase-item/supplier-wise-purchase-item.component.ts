import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-supplier-wise-purchase-item",
  templateUrl: "./supplier-wise-purchase-item.component.html",
  styleUrls: ["./supplier-wise-purchase-item.component.scss"],
})
export class SupplierWisePurchaseItemComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private storeService: StoreService
  ) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  stores: Store[];

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSuppliers();
    this.getAllStores();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      supplierId: [null],
      storeId: [null],
      reportType: [1, Validators.required],
    });
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.searchForm.get("supplierId");
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

  getAllStores() {
    let storeRequest = new StoreRequest();
    storeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.storeService.getStores(storeRequest).subscribe((res) => {
      this.stores = res?.data.item1;
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "supplierId") {
      this.searchForm?.get("supplierId").setValue(null);
    }
  }

  printPdf($event) {
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
        environment.apiURL +
          "/PurchaseInvoice/supplier-wise-purchase-item-print", //supplier-wise-purchase-item-print
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
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
          link.setAttribute("download", "supplier_wise_purchase_item.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
