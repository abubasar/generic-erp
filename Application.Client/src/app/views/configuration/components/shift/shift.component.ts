import { HttpClient } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { ToastrService } from "ngx-toastr";
import { Observable, merge, of } from "rxjs";
import { ShiftRequest } from "../../models/shift/shift-request.model";
import { Shift } from "../../models/shift/shift.model";
import { ShiftService } from "../../services/shift.service";
import { ShiftFormComponent } from "./shift-form/shift-form.component";

@Component({
  selector: "app-shift",
  templateUrl: "./shift.component.html",
  styleUrls: ["./shift.component.scss"],
})
export class ShiftComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  viewColumnForm: FormGroup;
  dataSource: MatTableDataSource<Shift>;
  totalCount: number;
  shiftRequest = new ShiftRequest();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;
  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "actions", label: "Actions" },
    { def: "name", label: "Name" },
    { def: "fromTime", label: "From Time" },
    { def: "toTime", label: "To Time" },
  ];

  constructor(
    private shiftService: ShiftService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getShifts(this.shiftRequest);
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
    this.viewColumnForm = this.fb.group({
      name: [true],
      fromTime: [true],
      toTime: [true],
      actions: [true],
    });
    this.updateDisplayedColumns();
  }

  private subscribeToFormChanges() {
    merge(this.viewColumnForm.valueChanges).subscribe(() => {
      this.updateDisplayedColumns();
    });
  }

  private updateDisplayedColumns() {
    this.displayedColumns$ = of(
      this.columnDefinitions
        .filter((x) => this.viewColumnForm.get(x.def).value)
        .map((col) => col.def)
    );
  }

  getShifts(request: ShiftRequest): void {
    this.shiftService.getShifts(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Shift>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.shiftService.deleteShift(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getShifts(this.shiftRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(shift?: Shift): void {
    const dialogRef = this.dialog.open(ShiftFormComponent, {
      disableClose: true,
      data: shift,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getShifts(this.shiftRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string) {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.searchForm.reset();
    this.loading = true;
    this.shiftRequest = new ShiftRequest();
    this.getShifts(this.shiftRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateShiftRequest();
    this.getShifts(this.shiftRequest);
  }

  onPageChange(pageEvent) {
    this.updateShiftRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getShifts(this.shiftRequest);
  }

  private updateShiftRequest(
    pageIndex = this.shiftRequest.page,
    pageSize = this.shiftRequest.rowsPerPage
  ) {
    this.shiftRequest = {
      ...this.shiftRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
}
