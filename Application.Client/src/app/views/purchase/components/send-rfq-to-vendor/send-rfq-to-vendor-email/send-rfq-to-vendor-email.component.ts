import { SelectionModel } from "@angular/cdk/collections";
import { Component, Inject, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { ActivatedRoute, Router } from "@angular/router";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { LocalStoreService } from "app/shared/services/local-store.service";
import { ProductTypeRequest } from "app/views/configuration/models/product-type/product-type-request.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { SupplierRequest } from "app/views/configuration/models/supplier/supplier-request.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { SendRFQRequestDTO } from "app/views/purchase/models/purchase-requisition/send-rfq-request-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";
import { ToastrService } from "ngx-toastr";
import { Observable, finalize, merge, of } from "rxjs";

@Component({
  selector: "app-send-rfq-to-vendor-email",
  templateUrl: "./send-rfq-to-vendor-email.component.html",
  styleUrls: ["./send-rfq-to-vendor-email.component.scss"],
})
export class SendRFQToVendorEmailComponent implements OnInit {
  isLoading: boolean = false;
  isDialogOpen: boolean = false;
  selection = new SelectionModel<Supplier>(true, []);
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  dataSource: MatTableDataSource<Supplier>;
  totalCount: number;
  supplierRequest = new SupplierRequest();
  productTypes: ProductType[];
  displayedColumns$: Observable<string[]>;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "select", label: "Select" },
    { def: "name", label: "Name" },
    { def: "nationalId", label: "NID" },
    { def: "tradeLicense", label: "Trade License" },
    { def: "contactNo", label: "Contact No" },
    { def: "email", label: "Email" },
    { def: "address", label: "Address" },
    { def: "bankName", label: "Bank Name" },
    { def: "bankBranchName", label: "Bank Branch Name" },
    { def: "bankAccountNo", label: "Bank Account No" },
    // { def: "supplierProductTypeId", label: "Supplier Product Type Id" },
  ];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PurchaseRequisitionResponseDTO,
    private dialogRef: MatDialogRef<SendRFQToVendorEmailComponent>,
    private purchaseRequisitionService: PurchaseRequisitionService,
    private productTypeServices: ProductTypeService,
    private supplierService: SupplierService,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router,
    private localStorageService: LocalStoreService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    // let id = this.route.snapshot.paramMap.get("id");
    // if (id) this.requisition = this.localStorageService.getItem(id);
    this.getSuppliers(this.supplierRequest);
    this.getAllProductTypes();
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
    this.viewColumnForm = this.fb.group({
      select: [true],
      name: [true],
      nationalId: [true],
      tradeLicense: [true],
      contactNo: [true],
      email: [true],
      address: [true],
      bankName: [false],
      bankBranchName: [false],
      bankAccountNo: [false],
      // supplierProductTypeId: [false],
    });
    this.updateDisplayedColumns();
  }

  private subscribeToFormChanges() {
    merge(this.viewColumnForm.valueChanges).subscribe(() => {
      this.updateDisplayedColumns();
    });
  }

  private updateDisplayedColumns() {
    this.displayedColumns$ = of(
      this.columnDefinitions
        .filter((x) => this.viewColumnForm.get(x.def).value)
        .map((col) => col.def)
    );
  }

  getSuppliers(requestBody): void {
    this.supplierService.getSuppliers(requestBody).subscribe((res) => {
      console.log(res);
      this.dataSource = new MatTableDataSource<Supplier>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
    });
  }

  getAllProductTypes(): void {
    let productTypeRequest = new ProductTypeRequest();
    // productTypeRequest.inventoryTypeId = Inventory_Type_Id.Raw_Materials;
    productTypeRequest.inventoryTypeId = Inventory_Type_Id_Raw_Materials;
    this.productTypeServices
      .getProductTypes(productTypeRequest)
      .subscribe((res) => {
        console.log(res);
        this.productTypes = res?.data?.item1;
      });
  }

  sendToVendor() {
    if (this.isLoading) return;
    this.isLoading = true;

    let arr = this.selection.selected.map((x) => x.id);
    let sendRFQRequest = new SendRFQRequestDTO();
    sendRFQRequest.purchaseRequisitionId = this.data.id;
    sendRFQRequest.selecetedSupplierIds = arr;

    this.purchaseRequisitionService
      .sendRFQToVendor(sendRFQRequest)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (res) => {
          this.selection = new SelectionModel<Supplier>(true, []);
          this.dialogRef.close();
          if (res?.succeeded) {
            this.data.requisitionStatus = res?.data as unknown as number;
            //  this.localStorageService.setItem(this.data.id,this.data);
            this.toastr.info(res?.message);
          } else {
            this.toastr.error(res?.message);
          }
        },
        error: () => {
          this.toastr.error("Something went wrong");
        },
      });
  }

  confirmSendToVendor() {
    if (this.isDialogOpen) return;
    this.isDialogOpen = true;

    const data: ConfirmDialogModel = {
      title: "Confirm Send to Vendor",
      message: `Are You Confirmed to Send RFQ to Selected Supplier?`,
    };
    const dialogRef = this.confirmDialogService.confirmDialog(
      "",
      this.sendToVendor.bind(this),
      data.message,
      data.title
    );

    dialogRef.afterClosed().subscribe(() => (this.isDialogOpen = false));
  }

  onSearch() {
    // this.isLoading = true;
    console.log(this.searchForm.value);
    this.supplierRequest.keyword = this.searchForm.get("keyword").value;
    this.getSuppliers(this.supplierRequest);
  }

  onPageChange(pageEvent) {
    this.supplierRequest.page = pageEvent.pageIndex;
    this.supplierRequest.rowsPerPage = pageEvent.pageSize;
    this.getSuppliers(this.searchForm.value);
  }
}
