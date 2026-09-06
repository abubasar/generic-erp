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
import { ManufacturingOrderStatus } from "app/shared/enums/manufacturingOrderStatus";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { ManufacturingOrderResponseDTO } from "app/views/production/models/manufacturing-order/manufacturing-order-response-dto.model";
import { ManufacturingOrderSearchRequestDTO } from "app/views/production/models/manufacturing-order/manufacturing-order-search-request-dto.model";
import { ManufacturingOrderService } from "app/views/production/services/manufacturing-order.service";

@Component({
  selector: "app-manufacturing-order-list",
  templateUrl: "./manufacturing-order-list.component.html",
  styleUrls: ["./manufacturing-order-list.component.scss"],
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
export class ManufacturingOrderListComponent implements OnInit {
  loading: boolean = true;
  selectedManufacturingOrder: ManufacturingOrderResponseDTO = null;
  filterManufacturingOrders: ManufacturingOrderResponseDTO[];
  lastFilterManufacturingOrders: ManufacturingOrderResponseDTO[];
  manufacturingOrdersNo: ManufacturingOrderResponseDTO[];
  filterFinishedProducts: ManufacturingOrderResponseDTO[];
  lastFilterBomsNoAndFormulationsNo: ManufacturingOrderResponseDTO[];
  filterBomsNoAndFormulationsNo: ManufacturingOrderResponseDTO[];
  bomsNoAndFormulationsNo: ManufacturingOrderResponseDTO[];
  finishedGoods: ManufacturingOrderResponseDTO[];
  manufacturingOrderRequest = new ManufacturingOrderSearchRequestDTO();
  searchForm: FormGroup;
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  isSearch: boolean = false;

  @ViewChild(MatPaginator) paginator: MatPaginator;

  displayedColumns: string[] = [
    "select", // its need for single selection
    "expand",
    "manufacturingOrderNo",
    "bomNo",
    "scheduledDate",
    "costCenterId",
    "formulationNo",
    "finishedProductId",
    "productionQuantity",
    "rawMaterialStoreId",
    "totalRmused",
    "status",
    "remark",
  ];
  dataSource: MatTableDataSource<ManufacturingOrderResponseDTO>;
  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private manufacturingOrderService: ManufacturingOrderService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getManufacturingOrders();
    this.getManufacturingOrdersNo();
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      finishedProductId: [null],
      manufacturingOrderNo: [""],
      formulationNo: [""],
      bomNo: [""],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.finishedProductId === "") {
        this.searchForm
          .get("finishedProductId")
          ?.patchValue(null, { emitEvent: false });
        if (this.searchForm?.value?.bomNo) {
          this.onSelectedBillOfMaterialNo(this.searchForm?.value?.bomNo);
        } else if (this.searchForm?.value?.formulationNo) {
          this.onSelectedFormulationNo(this.searchForm?.value?.formulationNo);
        } else {
          this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
            this.manufacturingOrdersNo;
          this.filterBomsNoAndFormulationsNo =
            this.lastFilterBomsNoAndFormulationsNo =
              this.bomsNoAndFormulationsNo;
        }
      }
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();

    if (fieldName === "finishedProductId") {
      this.searchForm.patchValue({
        finishedProductId: null,
      });
      if (this.searchForm?.value?.bomNo) {
        this.onSelectedBillOfMaterialNo(this.searchForm?.value?.bomNo);
      } else if (this.searchForm?.value?.formulationNo) {
        this.onSelectedFormulationNo(this.searchForm?.value?.formulationNo);
      } else {
        this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
          this.manufacturingOrdersNo;
        this.filterBomsNoAndFormulationsNo =
          this.lastFilterBomsNoAndFormulationsNo = this.bomsNoAndFormulationsNo;
      }
    }

    if (fieldName === "bomNo") {
      this.searchForm.patchValue({
        bomNo: "",
      });
      if (this.searchForm?.value?.formulationNo) {
        this.onSelectedFormulationNo(this.searchForm?.value?.formulationNo);
      } else if (this.searchForm?.value?.finishedProductId) {
        this.onSelectedFinishedProductId(
          this.searchForm?.value?.finishedProductId
        );
      } else {
        this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
          this.manufacturingOrdersNo;
        this.filterBomsNoAndFormulationsNo =
          this.lastFilterBomsNoAndFormulationsNo = this.bomsNoAndFormulationsNo;
      }
    }

    if (fieldName === "formulationNo") {
      this.searchForm.patchValue({
        formulationNo: "",
      });
      if (this.searchForm?.value?.bomNo) {
        this.onSelectedBillOfMaterialNo(this.searchForm?.value?.bomNo);
      } else if (this.searchForm?.value?.finishedProductId) {
        this.onSelectedFinishedProductId(
          this.searchForm?.value?.finishedProductId
        );
      } else {
        this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
          this.manufacturingOrdersNo;
        this.filterBomsNoAndFormulationsNo =
          this.lastFilterBomsNoAndFormulationsNo = this.bomsNoAndFormulationsNo;
      }
    }

    if (fieldName === "manufacturingOrderNo") {
      this.searchForm.patchValue({
        manufacturingOrderNo: "",
      });
    }
  }

  getManufacturingOrderStatus(value) {
    return this.statusColorService.getManufacturingOrderStatus(value);
  }

  getManufacturingOrderStatusName(value: number) {
    return ManufacturingOrderStatus[value];
  }

  getManufacturingOrders(): void {
    this.manufacturingOrderRequest.manufacturingOrderStatus = 3;
    this.manufacturingOrderService
      .getManufacturingOrders(this.manufacturingOrderRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<ManufacturingOrderResponseDTO>(
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

  getManufacturingOrdersNo(): void {
    this.manufacturingOrderRequest.manufacturingOrderStatus = 3;
    this.manufacturingOrderRequest.page = -1;
    this.manufacturingOrderService
      .getManufacturingOrders(this.manufacturingOrderRequest)
      .subscribe((res) => {
        const seen = new Set();
        const uniqueFinishedGoods = res?.data?.item1.filter(
          (manufacturingOrder) => {
            const isDuplicate = seen.has(manufacturingOrder.finishedProductId);
            seen.add(manufacturingOrder.finishedProductId);
            return !isDuplicate;
          }
        );
        const bomsNoAndFormulationsNoSeen = new Set();
        const uniqueBomsNoAndFormulationsNo = res?.data?.item1.filter(
          (manufacturingOrder) => {
            const isDuplicate = bomsNoAndFormulationsNoSeen.has(
              manufacturingOrder.bomNo
            );
            bomsNoAndFormulationsNoSeen.add(manufacturingOrder.bomNo);
            return !isDuplicate;
          }
        );

        this.filterManufacturingOrders =
          this.lastFilterManufacturingOrders =
          this.manufacturingOrdersNo =
            res?.data?.item1;

        this.filterFinishedProducts = this.finishedGoods = uniqueFinishedGoods;

        this.filterBomsNoAndFormulationsNo =
          this.lastFilterBomsNoAndFormulationsNo =
          this.bomsNoAndFormulationsNo =
            uniqueBomsNoAndFormulationsNo;
      });
  }

  handleFinishedGoodsSearch(): void {
    const term = this.searchForm
      ?.get("finishedProductId")
      ?.value?.toLowerCase();
    this.filterFinishedProducts = this.finishedGoods?.filter((mo) =>
      mo?.finishedProduct?.name.toLowerCase().includes(term)
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
      manufacturingOrderNo: "",
    });

    this.filterBomsNoAndFormulationsNo =
      this.lastFilterBomsNoAndFormulationsNo =
        this.bomsNoAndFormulationsNo?.filter(
          (x) => x.finishedProductId === finishedProductId
        );

    this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
      this.manufacturingOrdersNo?.filter(
        (x) => x.finishedProductId === finishedProductId
      );
  }

  onSelectedBillOfMaterialNo(bomNo: string) {
    this.searchForm.patchValue({
      manufacturingOrderNo: "",
    });
    const finishedProductId = this.searchForm?.value?.finishedProductId;
    if (finishedProductId) {
      this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
        this.manufacturingOrdersNo?.filter(
          (mo) =>
            mo.bomNo === bomNo && mo.finishedProductId === finishedProductId
        );
    } else {
      this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
        this.manufacturingOrdersNo?.filter((mo) => mo.bomNo === bomNo);
    }
  }

  onSelectedFormulationNo(formulationNo: string) {
    this.searchForm.patchValue({
      manufacturingOrderNo: "",
    });
    const finishedProductId = this.searchForm?.value?.finishedProductId;
    if (finishedProductId) {
      this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
        this.manufacturingOrdersNo?.filter(
          (mo) =>
            mo.formulationNo === formulationNo &&
            mo.finishedProductId === finishedProductId
        );
    } else {
      this.filterManufacturingOrders = this.lastFilterManufacturingOrders =
        this.manufacturingOrdersNo?.filter(
          (mo) => mo.formulationNo === formulationNo
        );
    }
  }

  handleBillOfMaterialNoSearch(): void {
    const term = this.searchForm?.get("bomNo")?.value?.toLowerCase();
    this.filterBomsNoAndFormulationsNo =
      this.lastFilterBomsNoAndFormulationsNo?.filter((mo) =>
        mo.bomNo.toLowerCase().includes(term)
      );
  }

  handleFormulationNoSearch(): void {
    const term = this.searchForm?.get("formulationNo")?.value?.toLowerCase();
    this.filterBomsNoAndFormulationsNo =
      this.lastFilterBomsNoAndFormulationsNo?.filter((mo) =>
        mo.formulationNo.toLowerCase().includes(term)
      );
  }

  handleManufacturingOrderNoSearch(): void {
    const term = this.searchForm
      ?.get("manufacturingOrderNo")
      ?.value?.toLowerCase();
    this.filterManufacturingOrders = this.lastFilterManufacturingOrders?.filter(
      (mo) => mo.manufacturingOrderNo.toLowerCase().includes(term)
    );
  }

  onSearch() {
    this.isSearch = true;
    this.loading = true;
    if (this.paginator) {
      this.paginator.firstPage();
    }
    this.manufacturingOrderRequest.page = 0;
    this.manufacturingOrderRequest = {
      ...this.manufacturingOrderRequest,
      ...this.searchForm.value,
    };
    this.getManufacturingOrders();
  }

  onPageChange(pageEvent) {
    if (!this.isSearch) {
      this.manufacturingOrderRequest.page = pageEvent.pageIndex;
      this.manufacturingOrderRequest.rowsPerPage = pageEvent.pageSize;
      this.getManufacturingOrders();
    }
  }

  handleRadioChange($event) {
    this.selectedManufacturingOrder = $event.value;
  }

  trackBy(index: number, item: any) {
    return item?.id;
  }
}
