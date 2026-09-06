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
import { MachineRequest } from "../../models/machine/machine-request.model";
import { Machine } from "../../models/machine/machine.model";
import { MachineService } from "../../services/machine.service";
import { MachineFormComponent } from "./machine-form/machine-form.component";

@Component({
  selector: "app-machine",
  templateUrl: "./machine.component.html",
  styleUrls: ["./machine.component.scss"],
})
export class MachineComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<Machine>;
  totalCount: number;
  machineRequest = new MachineRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private machineService: MachineService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getMachines(this.machineRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getMachines(request: MachineRequest): void {
    this.machineService.getMachines(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Machine>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.machineService.deleteMachine(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getMachines(this.machineRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(machine?: Machine): void {
    const dialogRef = this.dialog.open(MachineFormComponent, {
      disableClose: true,
      data: machine,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getMachines(this.machineRequest);
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
    this.machineRequest = new MachineRequest();
    this.getMachines(this.machineRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateMachineRequest();
    this.getMachines(this.machineRequest);
  }

  onPageChange(pageEvent) {
    this.updateMachineRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getMachines(this.machineRequest);
  }

  private updateMachineRequest(
    pageIndex = this.machineRequest.page,
    pageSize = this.machineRequest.rowsPerPage
  ) {
    this.machineRequest = {
      ...this.machineRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Machine/print", this.searchForm.value, {
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
