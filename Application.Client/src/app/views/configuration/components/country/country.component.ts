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
import { CountryRequest } from "../../models/country/country-request.model";
import { Country } from "../../models/country/country.model";
import { CountryService } from "../../services/country.service";
import { CountryFormComponent } from "./country-form/country-form.component";

@Component({
  selector: "app-country",
  templateUrl: "./country.component.html",
  styleUrls: ["./country.component.scss"],
})
export class CountryComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"]; //"code",
  dataSource: MatTableDataSource<Country>;
  totalCount: number;
  countryRequest = new CountryRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private countryService: CountryService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getCountries(this.countryRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getCountries(request: CountryRequest): void {
    this.countryService.getCountries(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Country>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.countryService.deleteCountry(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCountries(this.countryRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(country?: Country): void {
    const dialogRef = this.dialog.open(CountryFormComponent, {
      disableClose: true,
      data: country,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCountries(this.countryRequest);
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
    this.countryRequest = new CountryRequest();
    this.getCountries(this.countryRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCountryRequest();
    this.getCountries(this.countryRequest);
  }

  onPageChange(pageEvent) {
    this.updateCountryRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCountries(this.countryRequest);
  }

  private updateCountryRequest(
    pageIndex = this.countryRequest.page,
    pageSize = this.countryRequest.rowsPerPage
  ) {
    this.countryRequest = {
      ...this.countryRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Country/print", this.searchForm.value, {
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
