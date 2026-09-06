import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { GenericRequest } from "../../models/generic/generic-request.model";
import { Generic } from "../../models/generic/generic.model";
import { ProductType } from "../../models/product-type/product-type.model";
import { GenericService } from "../../services/generic.service";
import { ProductTypeService } from "../../services/product-type.service";
import { GenericFormComponent } from "./generic-form/generic-form.component";

@Component({
  selector: "app-generic",
  templateUrl: "./generic.component.html",
  styleUrls: ["./generic.component.scss"],
})
export class GenericComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "productType"];
  dataSource: MatTableDataSource<Generic>;
  totalCount: number;
  genericRequest = new GenericRequest();
  productTypes: ProductType[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private genericService: GenericService,
    private productTypeService: ProductTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getGenerics(this.genericRequest);
    this.getProductTypes();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      productTypeId: [null],
    });
  }

  getGenerics(requestBody: GenericRequest): void {
    this.genericService.getGenerics(requestBody).subscribe((res) => {
      console.log(res?.data?.item1);
      this.dataSource = new MatTableDataSource<Generic>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getProductTypes(): void {
    this.productTypeService.getAllProductTypes().subscribe((res) => {
      this.productTypes = res?.data?.item1;
      this.loading = false;
    });
  }

  remove(id): void {
    this.genericService.deleteGeneric(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getGenerics(this.genericRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(generic?: Generic): void {
    const dialogRef = this.dialog.open(GenericFormComponent, {
      disableClose: true,
      data: generic,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getGenerics(this.genericRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.genericRequest = new GenericRequest();
    this.getGenerics(this.genericRequest);
  }

  onSearch() {
    this.loading = true;
    const formValue = this.searchForm.value;
    this.genericRequest.keyword = formValue.keyword;
    this.genericRequest.productTypeId = formValue.productTypeId;
    this.getGenerics(this.genericRequest);
  }

  onPageChange(pageEvent) {
    this.genericRequest.page = pageEvent.pageIndex;
    this.genericRequest.rowsPerPage = pageEvent.pageSize;
    this.getGenerics(this.genericRequest);
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Generic/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
