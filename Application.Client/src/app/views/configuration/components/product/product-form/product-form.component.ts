import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { Inventory_Type_Id_Finished_Goods } from "app/shared/consts/const";
import settingConstants from "app/shared/consts/setting-constant";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { Category } from "app/views/configuration/models/category/category.model";
import { Country } from "app/views/configuration/models/country/country.model";
import { GenericRequest } from "app/views/configuration/models/generic/generic-request.model";
import { Generic } from "app/views/configuration/models/generic/generic.model";
import { InventoryTypeRequest } from "app/views/configuration/models/inventory-type/inventory-type-request.model";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";
import { Manufacturer } from "app/views/configuration/models/manufacturer/manufacturer.model";
import { MeasurementUnitRequest } from "app/views/configuration/models/measurement-unit/measurement-unit-request.model";
import { MeasurementUnit } from "app/views/configuration/models/measurement-unit/measurement-unit.model";
import { PackSize } from "app/views/configuration/models/pack-size/pack-size.model";
import { ProductTypeRequest } from "app/views/configuration/models/product-type/product-type-request.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { CategoryService } from "app/views/configuration/services/category.service";
import { CountryService } from "app/views/configuration/services/country.service";
import { GenericService } from "app/views/configuration/services/generic.service";
import { InventoryTypeService } from "app/views/configuration/services/inventory-type.service";
import { ManufacturerService } from "app/views/configuration/services/manufacturer.service";
import { MeasurementUnitService } from "app/views/configuration/services/measurement-unit.service";
import { PackSizeService } from "app/views/configuration/services/pack-size.service";
import { ProductTypeService } from "app/views/configuration/services/product-type.service";
import { ProductService } from "app/views/configuration/services/product.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-product-form",
  templateUrl: "./product-form.component.html",
  styleUrls: ["./product-form.component.scss"],
})
export class ProductFormComponent implements OnInit {
  Inventory_Type_Id_Finished_Goods = Inventory_Type_Id_Finished_Goods;
  Allow_Bag_Weight_Field_Show_When_Product_Entry =
    settingConstants.Allow_Bag_Weight_Field_Show_When_Product_Entry;
  businessType: string;
  formTitle: string;
  productForm: FormGroup;
  inventoryTypes: InventoryType[];
  categories: Category[];
  filteredCategories: Category[];
  packSizes: PackSize[];
  filteredPackSizes: PackSize[];
  mapProductTypes: ProductType[];
  productTypes: ProductType[];
  generics: Generic[];
  displayedGenerics: Generic[];
  //selectedProductTypeGenerics: Generic[];
  manufacturers: Manufacturer[];
  filteredManufacturers: Manufacturer[];
  countries: Country[];
  measurementUnits: MeasurementUnit[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: ProductView,
    private dialog: MatDialog,
    private productService: ProductService,
    private inventoryTypeService: InventoryTypeService,
    private categoryService: CategoryService,
    private packSizeService: PackSizeService,
    private productTypeService: ProductTypeService,
    private genericService: GenericService,
    private manufacturerService: ManufacturerService,
    private countryService: CountryService,
    private measurementUnitService: MeasurementUnitService,
    private jwtAuth: JwtAuthService,

    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    if (this.businessType === "1") {
      this.getAllCategories();
      this.getAllPackSizes();
    }
    this.initializeForm();
    this.getAllInventoryTypes();
    this.getAllProductTypes();
    this.getAllCountries();
    this.getAllGenerics();
    this.getAllManufacturers();
    this.getAllMeasurementUnits();
  }

  initializeForm() {
    this.productForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      genericId: [this.data?.genericId ?? null],
      composition: [this.data?.composition ?? ""],
      inventoryTypeId: [this.data?.inventoryTypeId, Validators.required],
      categoryId: [this.data?.categoryId ?? null],
      productTypeId: [this.data?.productTypeId, Validators.required],
      countryId: [this.data?.countryId ?? null],
      isPurchaseProduct: [this.data?.isPurchaseProduct ?? false],
      isSaleProduct: [this.data?.isSaleProduct ?? false],
      packSizeId: [this.data?.packSizeId ?? null],
      bagWeight: [this.data?.bagWeight ?? 1], //! default value 1
      manufacturerId: [this.data?.manufacturerId ?? null],
      vatPercentage: [this.data?.vatPercentage ?? 0],
      measurementUnitId: [this.data?.measurementUnitId, Validators.required],
      salePrice: [this.data?.salePrice ?? 0],
      mrp: [this.data?.mrp ?? 0],
      purchasePrice: [this.data?.purchasePrice ?? 0],
      alertQuantity: [this.data?.alertQuantity, Validators.required],
    });

    if (
      this.productForm.get("id").value === "" ||
      this.productForm.get("id").value == null
    ) {
      this.formTitle = "Add Product";
    } else {
      this.formTitle = "Edit Product";
    }

    this.productForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.genericId === "") {
        this.productForm
          .get("genericId")
          ?.patchValue(null, { emitEvent: false });
      }
      if (value.manufacturerId === "") {
        this.productForm
          .get("manufacturerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  getAllInventoryTypes(): void {
    let inventoryTypeRequest = new InventoryTypeRequest();
    inventoryTypeRequest.page = -1;
    this.inventoryTypeService
      .getInventoryTypes(inventoryTypeRequest)
      .subscribe((res) => {
        this.inventoryTypes = res?.data?.item1;
      });
  }

  getAllCategories(): void {
    this.categoryService.getAllCategories().subscribe((res) => {
      this.categories = this.filteredCategories = res?.data?.item1;
    });
  }

  getAllPackSizes(): void {
    this.packSizeService.getAllPackSizes().subscribe((res) => {
      this.filteredPackSizes = this.packSizes = res?.data?.item1;
    });
  }

  getAllProductTypes(): void {
    let productTypeRequest = new ProductTypeRequest();
    productTypeRequest.page = -1;
    this.productTypeService
      .getProductTypes(productTypeRequest)
      .subscribe((res) => {
        this.mapProductTypes = res?.data?.item1;
        this.productTypes = res?.data?.item1;
      });
  }

  getAllCountries() {
    this.countryService.getAllCountries().subscribe((res) => {
      this.countries = res?.data?.item1;
    });
  }
  getAllGenerics() {
    this.genericService.getAllGenerics().subscribe((res) => {
      this.displayedGenerics = this.generics = res?.data?.item1;
    });
  }

  getAllManufacturers() {
    this.manufacturerService.getAllManufacturers().subscribe((res) => {
      this.manufacturers = this.filteredManufacturers = res?.data?.item1;
    });
  }

  getAllMeasurementUnits(): void {
    let measurementUnitRequest = new MeasurementUnitRequest();
    measurementUnitRequest.page = -1;
    this.measurementUnitService
      .getMeasurementUnits(measurementUnitRequest)
      .subscribe((res) => {
        this.measurementUnits = res?.data?.item1;
      });
  }

  isProductFinishedGoods() {
    return (
      this.productForm.get("inventoryTypeId").value ===
      Inventory_Type_Id_Finished_Goods
    );
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "genericId") {
      this.productForm?.get("genericId").setValue(null);
      if (this.productForm.value.productTypeId)
        this.displayedGenerics = this.generics.filter(
          (x) => x.productTypeId == this.productForm.value.productTypeId
        );
      else this.displayedGenerics = this.generics;
    }
    if (fieldName === "manufacturerId") {
      this.productForm?.get("manufacturerId").setValue(null);
      this.filteredManufacturers = this.manufacturers;
    }
    if (fieldName === "categoryId") {
      this.productForm?.get("categoryId").setValue(null);
      this.filteredCategories = this.categories;
    }
    if (fieldName === "packSizeId") {
      this.productForm?.get("packSizeId").setValue(null);
      this.filteredPackSizes = this.packSizes;
    }
  }

  handleGenericSearch(event: any): void {
    const name = event.target?.name;
    if (name === "genericId") {
      const term = this.productForm.get("genericId");
      this.filterGeneric(term.value || "");
    }
  }

  private filterGeneric(value: string) {
    const filterValue = value.trim().toLowerCase();
    if (this.productForm.value.productTypeId)
      this.displayedGenerics = this.generics.filter(
        (x) =>
          x.productTypeId == this.productForm.value.productTypeId &&
          x.name.toLowerCase().includes(filterValue)
      );
    else {
      this.displayedGenerics = this.generics?.filter((option) =>
        option.name.toLowerCase().includes(filterValue)
      );
    }
  }

  getGenericName(genericId) {
    if (!genericId) {
      return;
    }
    if (this.productForm.value.productTypeId) {
      const generic =
        this.generics?.find(
          (x) =>
            x?.id === genericId &&
            x.productTypeId == this.productForm.value.productTypeId
        ) || this.data?.generic;
      return generic?.name;
    } else {
      const generic =
        this.generics?.find((x) => x?.id === genericId) || this.data?.generic;
      return generic?.name;
    }
  }

  handleCategorySearch(event: any): void {
    const name = event.target?.name;
    if (name === "categoryId") {
      const term = this.productForm.get("categoryId");
      this.filterCategory(term.value || "");
    }
  }

  private filterCategory(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filteredCategories = this.categories?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getCategoryName(categoryId) {
    if (!categoryId) {
      return;
    }
    const category =
      this.categories?.find((x) => x?.id === categoryId) || this.data?.category;
    return category?.name;
  }

  handlePackSizeSearch(event: any): void {
    const name = event.target?.name;
    if (name === "packSizeId") {
      const term = this.productForm.get("packSizeId");
      this.filterPackSize(term.value || "");
    }
  }

  private filterPackSize(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filteredPackSizes = this.packSizes?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getPackSizeName(packSizeId) {
    if (!packSizeId) {
      return;
    }
    const packSize =
      this.packSizes?.find((x) => x?.id === packSizeId) || this.data?.packSize;
    return packSize?.name;
  }

  handleManufacturerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "manufacturerId") {
      const term = this.productForm.get("manufacturerId");
      this.filterManufacturer(term.value || "");
    }
  }

  private filterManufacturer(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredManufacturers = this.manufacturers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getManufacturerName(manufacturerId) {
    if (!manufacturerId) {
      return;
    }
    const manufacturer =
      this.manufacturers?.find((x) => x?.id === manufacturerId) ||
      this.data?.manufacturer;
    return manufacturer?.name;
  }

  onSelectedProductTypeId(id: string) {
    this.productForm.patchValue({
      genericId: null,
    });

    this.getAllGenericsByProductTypeId(id);
  }

  getAllGenericsByProductTypeId(productTypeId: string) {
    let genericRequest = new GenericRequest();
    genericRequest.page = -1;
    genericRequest.productTypeId = productTypeId;
    this.displayedGenerics = this.generics.filter(
      (x) => x.productTypeId == productTypeId
    );
  }

  onSelectedInventoryType(id: string) {
    this.mapProductTypes = this.productTypes.filter(
      (type) => type.inventoryTypeId === id
    );
  }

  addProduct(body): void {
    this.productService.createProduct(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateProduct(body): void {
    this.productService.updateProduct(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.productForm.value) {
      if (
        this.productForm.get("id").value === "" ||
        this.productForm.get("id").value == null
      ) {
        this.addProduct(this.productForm.value);
      } else {
        this.updateProduct(this.productForm.value);
      }
    }
  }
}
