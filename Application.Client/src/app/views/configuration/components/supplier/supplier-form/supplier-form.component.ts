import { COMMA, ENTER } from "@angular/cdk/keycodes";
import {
  Component,
  ElementRef,
  Inject,
  OnInit,
  ViewChild,
} from "@angular/core";
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from "@angular/forms";
import { MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { MatChipInputEvent } from "@angular/material/chips";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { Inventory_Type_Id_Raw_Materials } from "app/shared/consts/const";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import {
  BankAccount,
  Supplier,
} from "app/views/configuration/models/supplier/supplier.model";
import { ProductService } from "app/views/configuration/services/product.service";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-supplier-form",
  templateUrl: "./supplier-form.component.html",
  styleUrls: ["./supplier-form.component.scss"],
})
export class SupplierFormComponent implements OnInit {
  formTitle: string;
  supplierForm: FormGroup;
  products: ProductView[] = [];
  // -----------------
  separatorKeysCodes: number[] = [ENTER, COMMA];
  productCtrl = new FormControl("");
  filteredProducts: ProductView[];
  allProducts: ProductView[];
  selectedProductIds: string[] = [];
  @ViewChild("productInput") productInput: ElementRef<HTMLInputElement>;
  // -----------------

  constructor(
    @Inject(MAT_DIALOG_DATA)
    private data: Supplier,
    private dialog: MatDialog,
    private supplierService: SupplierService,
    private productService: ProductService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.getAllProducts();
  }

  initializeForm() {
    this.createForm();
    this.populateForm();
    this.setFormTitle();
  }

  createForm() {
    this.supplierForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      ownersName: [this.data?.ownersName, Validators.required],
      contactNo: [
        this.data?.contactNo,
        [Validators.required, Validators.pattern("^01[3-9]\\d{8}$")],
      ],
      email: [this.data?.email ?? ""],
      address: [this.data?.address ?? ""],
      nationalId: [this.data?.nationalId ?? ""],
      tradeLicense: [this.data?.tradeLicense ?? ""],
      contactPersonName: [this.data?.contactPersonName, Validators.required],
      contactPersonContactNo: [
        this.data?.contactPersonContactNo,
        [Validators.required, Validators.pattern("^01[3-9]\\d{8}$")],
      ],
      contactPersonEmail: [this.data?.contactPersonEmail ?? ""],
      contactPersonDesignation: [this.data?.contactPersonDesignation ?? ""],
      // Assuming suppliedProductIds can be an empty array
      suppliedProductIds: [this.data?.suppliedProductIds ?? []],
      bankAccounts: this.fb.array([]),
    });

    if (this.data) {
      this.selectedProductIds = this.data?.suppliedProductIds;
      console.log(this.selectedProductIds);
    }
  }

  get bankAccounts(): FormArray {
    return this.supplierForm.get("bankAccounts") as FormArray;
  }

  populateForm(): void {
    if (this.supplierForm.get("id").value) {
      this.populateBankAccounts(this.data);
    } else {
      this.addItem();
    }
  }

  populateBankAccounts(data: Supplier): void {
    data.bankAccounts.forEach((item: BankAccount) => this.addItem(item));
  }

  addItem(item?: BankAccount): void {
    this.bankAccounts.push(this.createBankAccount(item));
  }

  createBankAccount(item?: BankAccount): FormGroup {
    return this.fb.group({
      id: [item?.id ?? null],
      name: [item?.name ?? "", Validators.required],
      accNo: [item?.accNo ?? "", Validators.required],
      routingNo: [item?.routingNo ?? ""],
      bankName: [item?.bankName ?? "", Validators.required],
      branchName: [item?.branchName ?? "", Validators.required],
    });
  }

  setFormTitle(): void {
    if (this.supplierForm.get("id").value) {
      this.formTitle = "Edit Supplier";
    } else {
      this.formTitle = "Add Supplier";
    }
  }

  getAllProducts(): void {
    let productRequest = new ProductRequest();
    productRequest.page = -1;
    productRequest.orderBy = "name";
    productRequest.isAscending = true;
    //productRequest.inventoryTypeIds = [Inventory_Type_Id_Raw_Materials];
    productRequest.isPurchaseProduct=true;
    this.productService.getProducts(productRequest).subscribe((res) => {
      this.allProducts = res?.data?.item1;
      const ids = this.data?.suppliedProductIds;
      if (ids) {
        this.products = this.allProducts?.filter((item) =>
          ids.includes(item.id)
        );
      }
    });
  }

  handleSuccessfulSave(res: any) {
    if (res?.succeeded) {
      this.toastr.success(res?.message);
      this.dialog.closeAll();
    } else {
      this.toastr.error(res?.message);
    }
  }

  addSupplier(body): void {
    this.supplierService
      .createSupplier(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  updateSupplier(body): void {
    this.supplierService
      .updateSupplier(body)
      .subscribe((res) => this.handleSuccessfulSave(res));
  }

  deletedIds: string = "";
  onDeleteItem(id: string, itemIndex: number): void {
    if (id) this.deletedIds += `${id},`;
    this.bankAccounts.removeAt(itemIndex);
  }

  onSubmit() {
    if (this.supplierForm.valid) {
      // this.isLoading = true;
      const formValue = this.supplierForm.value;
      if (!formValue.id) {
        this.addSupplier(formValue);
      } else {
        formValue.deletedBankAccountIds = this.deletedIds;
        this.updateSupplier(formValue);
      }
    }
  }
  // ---------------Start Test Code---------------------

  add(event: MatChipInputEvent): void {
    // console.log(event.value);
    // const value = event.value;
    // // Add our fruit
    // if (value) {
    //   // this.products.push(value);
    // }
    // // Clear the input value
    // event.chipInput!.clear();
    // this.productCtrl.setValue(null);
  }

  remove(productId: string): void {
    this.products = this.products?.filter(
      (product) => product.id !== productId
    );
    this.selectedProductIds = this.selectedProductIds?.filter(
      (id) => id !== productId
    );
    this.setSupplierProductIds(this.selectedProductIds);
  }

  findProductById(id: string) {
    return this.allProducts?.find((product) => product?.id === id);
  }

  private _filterProduct(value: string) {
    const filterValue = value?.toLowerCase();
    return this.allProducts?.filter((product) =>
      product.name.toLowerCase().startsWith(filterValue)
    );
  }

  handleProductSearch(event: any) {
    const name = event?.target?.name;
    if (name === "productCtrl") {
      const term = event.target.value;
      this.filteredProducts = this._filterProduct(term || "");
    }
  }

  setSupplierProductIds(productIds) {
    this.supplierForm.patchValue({
      suppliedProductIds: productIds,
    });
  }

  handleProductSelection(event: MatAutocompleteSelectedEvent) {
    const productId = event.option.value;
    const selectedProduct = this.findProductById(productId);
    console.log(this.selectedProductIds);
    this.selectedProductIds.push(selectedProduct.id);
    console.log(this.selectedProductIds);
    this.products.push(selectedProduct);
    this.productInput.nativeElement.value = "";
    this.productCtrl.setValue(null);

    if (!selectedProduct) return;

    this.setSupplierProductIds(this.selectedProductIds);
  }
}
