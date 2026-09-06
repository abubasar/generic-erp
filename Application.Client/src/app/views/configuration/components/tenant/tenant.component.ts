import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { ToastrService } from "ngx-toastr";
import { TenantRequest } from "../../models/tenant/tenant-request.model";
import { Tenant } from "../../models/tenant/tenant.model";
import { TenantService } from "../../services/tenant.service";
import { TenantFormComponent } from "./tenant-form/tenant-form.component";

@Component({
  selector: "app-tenant",
  templateUrl: "./tenant.component.html",
  styleUrls: ["./tenant.component.scss"],
})
export class TenantComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "code",
    "name",
    "timeZoneId",
    "address",
    "contactNo",
    "binno",
    "email",
    // "actions",
  ];
  dataSource: MatTableDataSource<Tenant>;
  totalCount: number;
  tenantRequest = new TenantRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private tenantService: TenantService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getTenants(this.tenantRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [null],
    });
  }

  getTenants(request: TenantRequest): void {
    this.tenantService.getTenants(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Tenant>(res?.data?.item1);
      console.log(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    // this.tenantService.deleteTenant(id).subscribe((res) => {
    //   if (res?.succeeded) {
    //     this.getTenants(this.tenantRequest);
    //     this.toastr.info(res?.message);
    //   } else {
    //     this.toastr.error(res?.message);
    //   }
    // });
  }
  openForm(tenant?: Tenant): void {
    const dialogRef = this.dialog.open(TenantFormComponent, {
      disableClose: true,
      data: tenant,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getTenants(this.tenantRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // // Open a confirmation dialog with a custom message and callback function
    // const data: ConfirmDialogModel = {
    //   title: "Confirm Remove",
    //   message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    // };
    // this.confirmDialogService.confirmDialog(
    //   id,
    //   this.remove.bind(this),
    //   data.message
    // );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.tenantRequest = new TenantRequest();
    this.getTenants(this.tenantRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateTenantRequest();
    this.getTenants(this.tenantRequest);
  }

  onPageChange(pageEvent) {
    this.updateTenantRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getTenants(this.tenantRequest);
  }

  private updateTenantRequest(
    pageIndex = this.tenantRequest.page,
    pageSize = this.tenantRequest.rowsPerPage
  ) {
    this.tenantRequest = {
      ...this.tenantRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    // this.http
    //   .post(environment.apiURL + "/Tenant/print", this.searchForm.value, {
    //     responseType: "blob",
    //   })
    //   .subscribe((response) => {
    //     //Create a Blob from the PDF Stream
    //     const file = new Blob([response], { type: "application/pdf" });
    //     //Build a URL from the file
    //     const fileURL = URL.createObjectURL(file);
    //     //Open the URL on new Window
    //     const pdfWindow = window.open();
    //     pdfWindow.location.href = fileURL;
    //   });
  }
}
