import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { StoreService } from "app/views/configuration/services/store.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-grn-supplier-item-wise-report",
  templateUrl: "./grn-supplier-item-wise-report.component.html",
  styleUrls: ["./grn-supplier-item-wise-report.component.scss"],
})
export class GrnSupplierItemWiseReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private purchaseOrderService: PurchaseOrderService,
    private storeService: StoreService
  ) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  stores: Store[];
  purchaseOrders: PurchaseOrderResponseDTO[];
  purchaseOrdersNo: PurchaseOrderResponseDTO[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();

  ngOnInit(): void {
    this.initializeForm();
    this.getAllSuppliers();
    this.getAllStores();
    this.getPurchaseOrdersNo();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      supplierId: [null],
      storeId: [null],
      ponumber: [""],
      reportType: [1, Validators.required],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.supplierId === "") {
        this.purchaseOrdersNo = this.purchaseOrders;
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  getPurchaseOrdersNo(): void {
    this.purchaseOrderRequest.page = -1;
    this.purchaseOrderRequest.purchaseOrderStatuses = [5, 6, 7];
    this.purchaseOrderService
      .getPurchaseOrders(this.purchaseOrderRequest)
      .subscribe((res) => {
        this.purchaseOrdersNo = this.purchaseOrders = res?.data?.item1;
      });
  }

  onPONoChange(): void {
    const term = this.searchForm.get("ponumber");
    this._filterPONo(term.value || "");
  }

  private _filterPONo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    if (supplierId) {
      this.purchaseOrdersNo = this.purchaseOrders?.filter(
        (x) =>
          x.ponumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.purchaseOrdersNo = this.purchaseOrders?.filter((x) =>
        x.ponumber.toLowerCase().includes(filterValue)
      );
    }
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
      this.purchaseOrdersNo = this.purchaseOrders;
    }
  }

  onSelectedSupplierId(supplierId: string) {
    this.searchForm.patchValue({
      ponumber: "",
    });
    this.purchaseOrdersNo = this.purchaseOrders?.filter(
      (purchaseOrder) => purchaseOrder.supplierId === supplierId
    );
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
        environment.apiURL + "/goodsReceiveNote/grn-supplier-item-wise-print",
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
          link.setAttribute("download", "grn_supplier_item_wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
