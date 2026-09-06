import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { GRNStatus } from "app/shared/enums/grnStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { GoodsReceiveNoteResponseDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-response-dto.model";
import { GoodsReceiveNoteSearchRequestDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-search-request-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { GoodsReceiveNoteService } from "app/views/purchase/services/goods-receive-note.service";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";

@Component({
  selector: "app-grn-list",
  templateUrl: "./grn-list.component.html",
  styleUrls: ["./grn-list.component.scss"],
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
export class GRNListComponent implements OnInit {
  loading: boolean = true;
  selection = new SelectionModel<GoodsReceiveNoteResponseDTO>(true, []); // its need for multiple selection
  goodsReceiveNotes: GoodsReceiveNoteResponseDTO[];
  filteredGoodsReceiveNotes: GoodsReceiveNoteResponseDTO[];
  goodsReceiveNoteRequest = new GoodsReceiveNoteSearchRequestDTO();
  purchaseOrdersNo: GoodsReceiveNoteResponseDTO[];

  filterPurchaseOrders: GoodsReceiveNoteResponseDTO[];
  filterPurchaseOrdersGRN: GoodsReceiveNoteResponseDTO[];
  purchaseOrderRequest = new PurchaseOrderSearchRequestDTO();

  filterLcNumbers: GoodsReceiveNoteResponseDTO[];

  panelOpenState = false;
  searchForm: FormGroup;
  filterSuppliers: GoodsReceiveNoteResponseDTO[];
  filterSuppliersGRN: GoodsReceiveNoteResponseDTO[];
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
    "lcNumber",
    "storeId",
    "supplierId",
    "challanNo",
    "challanDate",
    "truckNo",
    "driverName",
    "subtotal",
    "discount",
    "total",
    "remark",
    "status",
  ];
  dataSource: MatTableDataSource<GoodsReceiveNoteResponseDTO>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public isImportPurchaseData: boolean,
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private goodsReceiveNoteService: GoodsReceiveNoteService,
    private purchaseOrderService: PurchaseOrderService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder,
    private snackBarService: SnackBarService
  ) {}

  ngOnInit(): void {
    this.getGoodsReceiveNotes();
    this.getGoodsReceiveNotesNo();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      grnno: [""],
      supplierId: [null],
      ponumber: [""],
      lcNumber: [""],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.supplierId === "") {
        this.makeFilteredDataUpdate();
        this.searchForm
          .get("supplierId")
          ?.patchValue(null, { emitEvent: false });
      }

      if (value.ponumber === "") {
        const supplierId = this.searchForm.value.supplierId;
        const lcNo = this.searchForm.value.lcNumber;
        if (supplierId && !lcNo) {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
            (x) => x.supplierId === supplierId
          );
        } else if (lcNo) {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
            (x) => x.lcNumber === lcNo
          );
        } else {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes;
        }
        this.searchForm.get("ponumber")?.patchValue("", { emitEvent: false });
      }

      if (value.lcNumber === "") {
        const supplierId = this.searchForm.value.supplierId;
        const poNo = this.searchForm.value.ponumber;
        if (supplierId && !poNo) {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
            (x) => x.supplierId === supplierId
          );
        } else if (poNo) {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
            (x) => x.ponumber === poNo
          );
        } else {
          this.filteredGoodsReceiveNotes = this.goodsReceiveNotes;
        }
        this.searchForm.get("lcNumber")?.patchValue("", { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("supplierId").setValue(null);
    this.makeFilteredDataUpdate();
  }

  makeFilteredDataUpdate() {
    this.filterSuppliers = this.filterSuppliersGRN;
    this.filterPurchaseOrders = this.filterLcNumbers =
      this.filterPurchaseOrdersGRN;
    const poNo = this.searchForm.value.ponumber;
    const lcNo = this.searchForm.value.lcNumber;
    if (!poNo && !lcNo) {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes;
    }
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
    this.filterSuppliers = this.filterSuppliersGRN?.filter((option) =>
      option?.supplier.name.toLowerCase().includes(filterValue)
    );
  }

  getSupplierName(supplierId: string) {
    if (!supplierId) {
      return;
    }
    const supplierAccount = this.filterSuppliersGRN?.find(
      (grn) => grn?.supplierId === supplierId
    );
    return supplierAccount?.supplier?.name;
  }

  onSelectedSupplierId(supplierId: string) {
    this.searchForm.patchValue({
      ponumber: "",
      lcNumber: "",
      grnno: "",
    });
    this.filterPurchaseOrders = this.filterLcNumbers =
      this.filterPurchaseOrdersGRN?.filter((x) => x.supplierId === supplierId);
    this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
      (x) => x.supplierId == supplierId
    );
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
    this.goodsReceiveNoteRequest.page = -1;
    this.goodsReceiveNoteRequest.grnStatus = 3;
    this.goodsReceiveNoteService
      .getGoodsReceiveNotes(this.goodsReceiveNoteRequest)
      .subscribe((res) => {
        const grnData = res?.data?.item1 || [];
        const filteredGRNs = grnData?.filter(
          (grn) => grn.isImportPurchase === this.isImportPurchaseData
        );
        this.dataSource = new MatTableDataSource<GoodsReceiveNoteResponseDTO>(
          filteredGRNs
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getGoodsReceiveNotesNo(): void {
    this.goodsReceiveNoteRequest.page = -1;
    this.goodsReceiveNoteRequest.grnStatus = 3;
    this.goodsReceiveNoteService
      .getGoodsReceiveNotes(this.goodsReceiveNoteRequest)
      .subscribe((res) => {
        const grnData = res?.data?.item1 || [];
        const filteredGRNs = grnData?.filter(
          (grn) => grn.isImportPurchase === this.isImportPurchaseData
        );

        const seenSupplier = new Set();
        const uniqueSuppliersGRN = filteredGRNs.filter((grn) => {
          const isDuplicate = seenSupplier.has(grn.supplierId);
          seenSupplier.add(grn.supplierId);
          return !isDuplicate;
        });
        this.filterSuppliers = this.filterSuppliersGRN = uniqueSuppliersGRN;

        const seenPO = new Set();
        const uniquePurchaseOrdersGRN = filteredGRNs.filter((grn) => {
          const isDuplicate = seenPO.has(grn.ponumber);
          seenPO.add(grn.ponumber);
          return !isDuplicate;
        });
        this.filterPurchaseOrders =
          this.filterLcNumbers =
          this.filterPurchaseOrdersGRN =
            uniquePurchaseOrdersGRN;

        this.filteredGoodsReceiveNotes = this.goodsReceiveNotes = filteredGRNs;
      });
  }

  onPONoChange(): void {
    const term = this.searchForm.get("ponumber");
    this.filterPONo(term.value || "");
  }

  private filterPONo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    const lcNo = this.searchForm.value.lcNumber;
    if (supplierId && !lcNo) {
      this.filterPurchaseOrders = this.filterPurchaseOrdersGRN?.filter(
        (x) =>
          x.ponumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else if (lcNo) {
      this.filterPurchaseOrders = this.filterPurchaseOrdersGRN?.filter(
        (x) =>
          x.ponumber.toLowerCase().includes(filterValue) && x.lcNumber === lcNo
      );
    } else {
      this.filterPurchaseOrders = this.filterPurchaseOrdersGRN?.filter((x) =>
        x.ponumber.toLowerCase().includes(filterValue)
      );
    }
  }

  onLcNoChange(): void {
    const term = this.searchForm.get("lcNumber");
    this.filterLcNo(term.value || "");
  }

  private filterLcNo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    const poNo = this.searchForm.value.ponumber;
    if (supplierId && !poNo) {
      this.filterLcNumbers = this.filterPurchaseOrdersGRN?.filter(
        (x) =>
          x.lcNumber.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else if (poNo) {
      this.filterLcNumbers = this.filterPurchaseOrdersGRN?.filter(
        (x) =>
          x.lcNumber.toLowerCase().includes(filterValue) && x.ponumber === poNo
      );
    } else {
      this.filterLcNumbers = this.filterPurchaseOrdersGRN?.filter((x) =>
        x.lcNumber.toLowerCase().includes(filterValue)
      );
    }
  }

  onGRNNoChange(): void {
    const term = this.searchForm.get("grnno");
    this.filterGRN(term.value || "");
  }

  private filterGRN(value: string) {
    const filterValue = value.toLowerCase().trim();
    const supplierId = this.searchForm.value.supplierId;
    const poNo = this.searchForm.value.ponumber;
    const lcNo = this.searchForm.value.lcNumber;
    if (poNo || lcNo) {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
        (x) =>
          x.grnno.toLowerCase().includes(filterValue) &&
          (x.ponumber === poNo || x.lcNumber === lcNo)
      );
    } else if (supplierId && !poNo && !lcNo) {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
        (x) =>
          x.grnno.toLowerCase().includes(filterValue) &&
          x.supplierId === supplierId
      );
    } else {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter((x) =>
        x.grnno.toLowerCase().includes(filterValue)
      );
    }
  }

  onSelectedPONo(poNo: string) {
    this.searchForm.patchValue({
      grnno: "",
    });
    const lcNo = this.searchForm.value.lcNumber;
    if (!lcNo) {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
        (x) => x.ponumber == poNo
      );
    }
    this.filterLcNumbers = this.filterPurchaseOrdersGRN?.filter(
      (x) => x.ponumber == poNo
    );
  }

  onSelectedLcNo(lcNo: string) {
    this.searchForm.patchValue({
      grnno: "",
    });
    const poNo = this.searchForm.value.ponumber;
    if (!poNo) {
      this.filteredGoodsReceiveNotes = this.goodsReceiveNotes?.filter(
        (x) => x.lcNumber == lcNo
      );
    }
    this.filterPurchaseOrders = this.filterPurchaseOrdersGRN?.filter(
      (x) => x.lcNumber == lcNo
    );
  }

  onSearch() {
    this.loading = true;
    this.goodsReceiveNoteRequest = {
      ...this.goodsReceiveNoteRequest,
      ...this.searchForm.value,
    };
    this.getGoodsReceiveNotes();
  }

  isDuplicatePONumber: boolean = false;
  checkUniquePONumber() {
    const selectedPONumbers = this.selection.selected.map(
      (note) => note.ponumber
    );
    const uniquePONumbers = new Set(selectedPONumbers);
    if (uniquePONumbers.size > 1) {
      // Display alert message
      this.showSnackBar(
        "You cannot select multiple different PO numbers at a time"
      );
      this.isDuplicatePONumber = true;
    } else this.isDuplicatePONumber = false;
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }
}
