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
import { TerritoryRequest } from "../../models/Territory/territory-request.model";
import { Territory } from "../../models/Territory/territory.model";
import { Area } from "../../models/area/area.model";
import { AreaService } from "../../services/area.service";
import { TerritoryService } from "../../services/territory.service";
import { TerritoryFormComponent } from "./territory-form/territory-form.component";

@Component({
  selector: "app-territory",
  templateUrl: "./territory.component.html",
  styleUrls: ["./territory.component.scss"],
})
export class TerritoryComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "area"];
  dataSource: MatTableDataSource<Territory>;
  totalCount: number;
  territoryRequest = new TerritoryRequest();
  areas: Area[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private territoryService: TerritoryService,
    private areaService: AreaService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getTerritories(this.territoryRequest);
    this.getAreas();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      areaId: [null],
    });
  }

  getTerritories(requestBody: TerritoryRequest): void {
    this.territoryService.getTerritories(requestBody).subscribe((res) => {
      console.log(res?.data?.item1);
      this.dataSource = new MatTableDataSource<Territory>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
      console.log("Length----:     ", this.dataSource.data.length);
    });
  }

  getAreas(): void {
    this.areaService.getAllAreas().subscribe((res) => {
      this.areas = res?.data?.item1;
      this.loading = false;
    });
  }

  remove(id): void {
    this.territoryService.deleteTerritory(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getTerritories(this.territoryRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(territory?: Territory): void {
    const dialogRef = this.dialog.open(TerritoryFormComponent, {
      disableClose: true,
      data: territory,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getTerritories(this.territoryRequest);
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
    this.territoryRequest = new TerritoryRequest();
    this.getTerritories(this.territoryRequest);
  }

  onSearch() {
    this.loading = true;
    const formValue = this.searchForm.value;
    this.territoryRequest.keyword = formValue.keyword;
    this.territoryRequest.areaId = formValue.areaId;
    this.getTerritories(this.territoryRequest);
  }

  onPageChange(pageEvent) {
    this.territoryRequest.page = pageEvent.pageIndex;
    this.territoryRequest.rowsPerPage = pageEvent.pageSize;
    this.getTerritories(this.territoryRequest);
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Territory/print", this.searchForm.value, {
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
