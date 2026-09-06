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
import { Region } from "../../models/region/region.model";
import { ZoneRequest } from "../../models/zone/zone-request.model";
import { Zone } from "../../models/zone/zone.model";
import { RegionService } from "../../services/region.service";
import { ZoneService } from "../../services/zone.service";
import { ZoneFormComponent } from "./zone-form/zone-form.component";

@Component({
  selector: "app-zone",
  templateUrl: "./zone.component.html",
  styleUrls: ["./zone.component.scss"],
})
export class ZoneComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "region"];
  dataSource: MatTableDataSource<Zone>;
  totalCount: number;
  zoneRequest = new ZoneRequest();
  regions: Region[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private zoneService: ZoneService,
    private regionService: RegionService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getZones(this.zoneRequest);
    this.getRegions();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      regionId: [null],
    });
  }

  getZones(requestBody: ZoneRequest): void {
    this.zoneService.getZones(requestBody).subscribe((res) => {
      console.log(res?.data?.item1);
      this.dataSource = new MatTableDataSource<Zone>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getRegions(): void {
    this.regionService.getAllRegions().subscribe((res) => {
      this.regions = res?.data?.item1;
      this.loading = false;
    });
  }

  remove(id): void {
    this.zoneService.deleteZone(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getZones(this.zoneRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(zone?: Zone): void {
    const dialogRef = this.dialog.open(ZoneFormComponent, {
      disableClose: true,
      data: zone,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getZones(this.zoneRequest);
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
    this.zoneRequest = new ZoneRequest();
    this.getZones(this.zoneRequest);
  }

  onSearch() {
    this.loading = true;
    const formValue = this.searchForm.value;
    this.zoneRequest.keyword = formValue.keyword;
    this.zoneRequest.regionId = formValue.regionId;
    this.getZones(this.zoneRequest);
  }

  onPageChange(pageEvent) {
    this.zoneRequest.page = pageEvent.pageIndex;
    this.zoneRequest.rowsPerPage = pageEvent.pageSize;
    this.getZones(this.zoneRequest);
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Zone/print", this.searchForm.value, {
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
