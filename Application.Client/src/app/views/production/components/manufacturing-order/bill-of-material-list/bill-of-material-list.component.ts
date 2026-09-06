import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatPaginator } from "@angular/material/paginator";
import { MatTableDataSource } from "@angular/material/table";
import { BOMStatus } from "app/shared/enums/bomStatus";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { BillOfMaterialResponseDTO } from "app/views/production/models/bill-of-material/bill-of-material-response-dto.model";
import { BillOfMaterialSearchRequestDTO } from "app/views/production/models/bill-of-material/bill-of-material-search-request-dto.model";
import { BillOfMaterialService } from "app/views/production/services/bill-of-material.service";

@Component({
  selector: "app-bill-of-material-list",
  templateUrl: "./bill-of-material-list.component.html",
  styleUrls: ["./bill-of-material-list.component.scss"],
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
export class BillOfMaterialListComponent implements OnInit {
  loading: boolean = true;
  selectedBillOfMaterial: BillOfMaterialResponseDTO = null;
  billOfMaterials: BillOfMaterialResponseDTO[];
  billOfMaterialsNo: BillOfMaterialResponseDTO[];
  filterFinishedProducts: BillOfMaterialResponseDTO[];
  finishedGoods: BillOfMaterialResponseDTO[];
  billOfMaterialRequest = new BillOfMaterialSearchRequestDTO();
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  isSearch: boolean = false;

  @ViewChild(MatPaginator) paginator: MatPaginator;

  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "bomNo",
    "finishedProductId",
    "formulationNo",
    "dosageQuantity",
    "status",
    "remark",
  ];
  dataSource: MatTableDataSource<BillOfMaterialResponseDTO>;
  constructor(
    public statusColorService: StatusColorService,
    private billOfMaterialService: BillOfMaterialService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getBillOfMaterials();
    this.getBillOfMaterialsNo();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      finishedProductId: [null],
      formulationNo: [""],
      bomNo: [""],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.finishedProductId === "") {
        this.billOfMaterials = this.billOfMaterialsNo;
        this.searchForm
          .get("finishedProductId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  getBOMStatus(value) {
    return this.statusColorService.getBOMStatus(value);
  }

  getBOMStatusName(value: number) {
    return BOMStatus[value];
  }

  getBillOfMaterials(): void {
    this.billOfMaterialRequest.bomStatus = 3;
    this.billOfMaterialService
      .getBillOfMaterials(this.billOfMaterialRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<BillOfMaterialResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        if (this.paginator) {
          this.paginator.length = this.totalCount;
        }
        this.loading = false;
        this.isSearch = false;
      });
  }

  getBillOfMaterialsNo(): void {
    this.billOfMaterialRequest.bomStatus = 3;
    this.billOfMaterialRequest.page = -1;
    this.billOfMaterialService
      .getBillOfMaterials(this.billOfMaterialRequest)
      .subscribe((res) => {
        const seen = new Set();
        const uniqueFinishedGoods = res?.data?.item1.filter((bom) => {
          const isDuplicate = seen.has(bom.finishedProductId);
          seen.add(bom.finishedProductId);
          return !isDuplicate;
        });
        this.billOfMaterials = this.billOfMaterialsNo = res?.data?.item1;
        this.filterFinishedProducts = this.finishedGoods = uniqueFinishedGoods;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "bomNo") {
      this.searchForm?.get("bomNo").setValue("");
    }
    if (fieldName === "formulationNo") {
      this.searchForm?.get("formulationNo").setValue("");
    }
    if (fieldName === "finishedProductId") {
      this.searchForm?.get("finishedProductId").setValue(null);
      this.billOfMaterials = this.billOfMaterialsNo;
    }
  }

  handleFinishedGoodsSearch(event: any): void {
    const name = event.target?.name;
    if (name === "finishedProductId") {
      const term = this.searchForm.get("finishedProductId");
      this._filterFinishedGoods(term.value || "");
    }
  }

  private _filterFinishedGoods(value: string) {
    const filterValue = value.toLowerCase();
    this.filterFinishedProducts = this.finishedGoods?.filter((option) =>
      option?.finishedProduct?.name.toLowerCase().includes(filterValue)
    );
  }

  getFinishedGoodsName(finishedProductId: string) {
    if (!finishedProductId) {
      return;
    }
    const finishedGoodsAccount = this.finishedGoods?.find(
      (finishedGood) => finishedGood?.finishedProductId === finishedProductId
    );
    return finishedGoodsAccount?.finishedProduct?.name;
  }

  onSelectedFinishedProductId(finishedProductId: string) {
    this.searchForm.patchValue({
      bomNo: "",
      formulationNo: "",
    });
    if (this.billOfMaterials) {
      this.billOfMaterials = this.billOfMaterialsNo?.filter(
        (x) => x.finishedProductId === finishedProductId
      );
    }
  }

  onBillOfMaterialNoChange(): void {
    const term = this.searchForm.get("bomNo");
    this._filterBillOfMaterials(term.value || "");
  }

  private _filterBillOfMaterials(value: string) {
    const filterValue = value.toLowerCase().trim();
    const finishedProductId = this.searchForm.value.finishedProductId;
    if (finishedProductId) {
      this.billOfMaterials = this.billOfMaterialsNo?.filter(
        (x) =>
          x.bomNo.toLocaleLowerCase().includes(filterValue) &&
          x.finishedProductId === finishedProductId
      );
    } else {
      this.billOfMaterials = this.billOfMaterialsNo?.filter((x) =>
        x.bomNo.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onFormulationNoChange(): void {
    const term = this.searchForm.get("formulationNo");
    this._filterFormulationNo(term.value || "");
  }

  private _filterFormulationNo(value: string) {
    const filterValue = value.toLowerCase().trim();
    const finishedProductId = this.searchForm.value.finishedProductId;
    if (finishedProductId) {
      this.billOfMaterials = this.billOfMaterialsNo?.filter(
        (x) =>
          x.formulationNo.toLocaleLowerCase().includes(filterValue) &&
          x.finishedProductId === finishedProductId
      );
    } else {
      this.billOfMaterials = this.billOfMaterialsNo?.filter((x) =>
        x.formulationNo.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onSearch() {
    this.isSearch = true;
    this.loading = true;
    if (this.paginator) {
      this.paginator.firstPage();
    }
    this.billOfMaterialRequest.page = 0;
    this.billOfMaterialRequest = {
      ...this.billOfMaterialRequest,
      ...this.searchForm.value,
    };
    this.getBillOfMaterials();
  }

  onPageChange(pageEvent) {
    if (!this.isSearch) {
      this.billOfMaterialRequest.page = pageEvent.pageIndex;
      this.billOfMaterialRequest.rowsPerPage = pageEvent.pageSize;
      this.getBillOfMaterials();
    }
  }

  handleRadioChange($event) {
    this.selectedBillOfMaterial = $event.value;
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
