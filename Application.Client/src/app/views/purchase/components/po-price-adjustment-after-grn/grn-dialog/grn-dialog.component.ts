import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import { GRNStatus } from "app/shared/enums/grnStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { SupplierService } from "app/views/configuration/services/supplier.service";
import { GoodsReceiveNoteResponseDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import { GoodsReceiveNoteSearchRequestDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-search-request-dto.model";
import { GoodsReceiveNoteService } from "app/views/purchase/services/goods-receive-note.service";

@Component({
  selector: "app-grn-dialog",
  templateUrl: "./grn-dialog.component.html",
  styleUrls: ["./grn-dialog.component.scss"],
  animations: [
    trigger("detailExpand", [
      state("collapsed", style({ height: "0px", minHeight: "0" })),
      state("expanded", style({ height: "*" })),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)")
      ),
    ]),
  ],
})
export class GrnDialogComponent implements OnInit {
  dialogTitle: string = "Select Goods Receive Note";
  loading: boolean = true;
  selectedGoodsReceiveNote: GoodsReceiveNoteResponseDTO = null;
  // selection = new SelectionModel<GoodsReceiveNoteResponseDTO>(true, []); // its need for multiple selection
  goodsReceiveNotes: GoodsReceiveNoteResponseDTO[];
  goodsReceiveNoteRequest = new GoodsReceiveNoteSearchRequestDTO();

  panelOpenState = false;
  searchForm: FormGroup;
  suppliers: Supplier[];
  filterSuppliers: Supplier[];
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  grnStatuses: ENUM[];
  displayedColumns: string[] = [
    "select", // its need for multiple selection
    "expand",
    "grnno",
    "grndate",
    "ponumber",
    "storeId",
    "supplierId",
    "challanNo",
    "challanDate",
    "truckNo",
    "driverName",
    "driverContactNo",
    "subtotal",
    "discount",
    "total",
    "remark",
    "status",
  ];
  dataSource: MatTableDataSource<GoodsReceiveNoteResponseDTO>;

  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private goodsReceiveNoteService: GoodsReceiveNoteService,
    private supplierService: SupplierService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getGoodsReceiveNotes();
    this.getAllSuppliers();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      grnno: [""],
      supplierId: [null],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.supplierId === "") {
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
  }

  handleSupplierSearch(event: any): void {
    const name = event.target?.name;
    if (name === "supplierId") {
      const term = this.searchForm.get("supplierId");
      this.filterSupplier(term.value || "");
    }
  }

  private filterSupplier(value: string) {
    const filterValue = value.toLowerCase();
    this.filterSuppliers = this.suppliers?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.suppliers?.find(
      (supplier) => supplier?.id === supplierId
    );
    return supplierAccount?.name;
  }

  getAllSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe((res) => {
      this.filterSuppliers = this.suppliers = res?.data?.item1;
    });
  }

  getAllGRNStatuses() {
    this.enumValueService.getGRNStatuses().subscribe((res) => {
      console.log("rq status:", res);
      this.grnStatuses = res;
    });
  }

  getGoodsReceiveNoteStatus(value) {
    return this.statusColorService.getGoodsReceiveNoteStatus(value);
  }

  getGoodsReceiveNoteName(value: number) {
    return GRNStatus[value];
  }

  getGoodsReceiveNotes(): void {
    this.goodsReceiveNoteRequest.grnStatuses = [3];
    //this.goodsReceiveNoteRequest.keyword = keyword;
    //this.goodsReceiveNoteRequest.rowsPerPage=5;
    this.goodsReceiveNoteService
      .getGoodsReceiveNotes(this.goodsReceiveNoteRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<GoodsReceiveNoteResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getGoodsReceiveNotesNo(keyword): void {
    this.goodsReceiveNoteRequest.grnStatuses = [3];
    this.goodsReceiveNoteRequest.keyword = keyword;
    this.goodsReceiveNoteService
      .getGoodsReceiveNotes(this.goodsReceiveNoteRequest)
      .subscribe((res) => {
        this.goodsReceiveNotes = res?.data?.item1;
      });
  }

  onGRNNoChange(): void {
    const term = this.searchForm.get("grnno");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.getGoodsReceiveNotesNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    this.goodsReceiveNoteRequest.keyword = this.searchForm.value.grnno;
    this.goodsReceiveNoteRequest.supplierId = this.searchForm.value.supplierId;
    //let grnno = this.searchForm.get("grnno").value;
    this.getGoodsReceiveNotes();
  }

  onPageChange(pageEvent) {
    this.goodsReceiveNoteRequest.page = pageEvent.pageIndex;
    this.goodsReceiveNoteRequest.rowsPerPage = pageEvent.pageSize;
    this.getGoodsReceiveNotes();
  }

  handleRadioChange($event) {
    this.selectedGoodsReceiveNote = $event.value;
  }
}
