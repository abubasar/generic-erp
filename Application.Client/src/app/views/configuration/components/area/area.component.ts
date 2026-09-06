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
import { AreaRequest } from "../../models/area/area-request.model";
import { Area } from "../../models/area/area.model";
import { Zone } from "../../models/zone/zone.model";
import { AreaService } from "../../services/area.service";
import { ZoneService } from "../../services/zone.service";
import { AreaFormComponent } from "./area-form/area-form.component";

@Component({
  selector: "app-area",
  templateUrl: "./area.component.html",
  styleUrls: ["./area.component.scss"],
})
export class AreaComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name", "zone"];
  dataSource: MatTableDataSource<Area>;
  totalCount: number;
  areaRequest = new AreaRequest();
  zones: Zone[];
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private areaService: AreaService,
    private zoneService: ZoneService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getAreas(this.areaRequest);
    this.getZones();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      zoneId: [null],
    });
  }

  getAreas(requestBody: AreaRequest): void {
    this.areaService.getAreas(requestBody).subscribe((res) => {
      console.log(res?.data?.item1);
      this.dataSource = new MatTableDataSource<Area>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  getZones(): void {
    this.zoneService.getAllZones().subscribe((res) => {
      this.zones = res?.data?.item1;
      this.loading = false;
    });
  }

  remove(id): void {
    this.areaService.deleteArea(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getAreas(this.areaRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(area?: Area): void {
    const dialogRef = this.dialog.open(AreaFormComponent, {
      disableClose: true,
      data: area,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getAreas(this.areaRequest);
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
    this.areaRequest = new AreaRequest();
    this.getAreas(this.areaRequest);
  }

  onSearch() {
    this.loading = true;
    const formValue = this.searchForm.value;
    this.areaRequest.keyword = formValue.keyword;
    this.areaRequest.zoneId = formValue.zoneId;
    this.getAreas(this.areaRequest);
  }

  onPageChange(pageEvent) {
    this.areaRequest.page = pageEvent.pageIndex;
    this.areaRequest.rowsPerPage = pageEvent.pageSize;
    this.getAreas(this.areaRequest);
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Area/print", this.searchForm.value, {
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
