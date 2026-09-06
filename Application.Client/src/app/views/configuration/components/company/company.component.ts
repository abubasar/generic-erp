import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { ToastrService } from "ngx-toastr";
import { CompanyRequest } from "../../models/company/company-request.model";
import { Company } from "../../models/company/company.model";
import { CompanyService } from "../../services/company.service";
import { CompanyFormComponent } from "./company-form/company-form.component";

@Component({
  selector: "app-company",
  templateUrl: "./company.component.html",
  styleUrls: ["./company.component.scss"],
})
export class CompanyComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "code", "name"];
  dataSource: MatTableDataSource<Company>;
  totalCount: number;
  companyRequest = new CompanyRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private companyService: CompanyService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getCompanies(this.companyRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getCompanies(request: CompanyRequest): void {
    this.companyService.getCompanies(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Company>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }
  remove(id): void {
    this.companyService.deleteCompany(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCompanies(this.companyRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(company?: Company): void {
    const dialogRef = this.dialog.open(CompanyFormComponent, {
      disableClose: true,
      data: company,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCompanies(this.companyRequest);
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
    this.companyRequest = new CompanyRequest();
    this.getCompanies(this.companyRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCompanyRequest();
    this.getCompanies(this.companyRequest);
  }

  onPageChange(pageEvent) {
    this.updateCompanyRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCompanies(this.companyRequest);
  }

  private updateCompanyRequest(
    pageIndex = this.companyRequest.page,
    pageSize = this.companyRequest.rowsPerPage
  ) {
    this.companyRequest = {
      ...this.companyRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
}
