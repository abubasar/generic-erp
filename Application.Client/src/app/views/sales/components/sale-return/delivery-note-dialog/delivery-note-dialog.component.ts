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
import { DeliveryNoteStatus } from "app/shared/enums/deliveryNoteStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { CustomerService } from "app/views/configuration/services/customer.service";
import { DeliveryNoteResponseDTO } from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import { DeliveryNoteSearchRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-search-request-dto.model";
import { DeliveryNoteService } from "app/views/sales/services/delivery-note.service";

@Component({
  selector: "app-delivery-note-dialog",
  templateUrl: "./delivery-note-dialog.component.html",
  styleUrls: ["./delivery-note-dialog.component.scss"],
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
export class DeliveryNoteDialogComponent implements OnInit {
  dialogTitle = "Select Delivery Note";
  loading: boolean = true;
  selectedDeliveryNote: DeliveryNoteResponseDTO = null;
  // selection = new SelectionModel<DeliveryNoteResponseDTO>(true, []); // its need for multiple selection
  deliveryNotes: DeliveryNoteResponseDTO[];
  deliveryNoteRequest = new DeliveryNoteSearchRequestDTO();

  panelOpenState = false;
  searchForm: FormGroup;
  customers: Customer[];
  filterCustomers: Customer[];
  expandedElement: any;
  expandedRow: any;
  totalCount: number;
  deliveryNoteStatuses: ENUM[];
  displayedColumns: string[] = [
    "select", // its need for multiple selection
    "expand",
    "saleOrderNo",
    "deliveryNoteNo",
    "customerId",
    "storeId",
    "referenceNo",
    "status",
  ];
  dataSource: MatTableDataSource<DeliveryNoteResponseDTO>;

  constructor(
    public dateFormatService: DateTimeFormatService,
    public statusColorService: StatusColorService,
    private deliveryNoteService: DeliveryNoteService,
    private customerService: CustomerService,
    private enumValueService: EnumValueService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getDeliveryNotes();
    this.getAllCustomers();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      deliveryNoteNo: [""],
      customerId: [null],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.customerId === "") {
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
    }
    if (fieldName === "deliveryNoteNo") {
      this.searchForm?.get("deliveryNoteNo").setValue("");
    }
  }

  handleCustomerSearch(event: any): void {
    const name = event.target?.name;
    if (name === "customerId") {
      const term = this.searchForm.get("customerId");
      this.filterCustomer(term.value || "");
    }
  }

  private filterCustomer(value: string) {
    const filterValue = value.trim().toLowerCase();
    this.filterCustomers = this.customers?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.id === customerId
    );
    return customerAccount?.name;
  }

  getAllCustomers() {
    this.customerService.getAllCustomers().subscribe((res) => {
      this.customers = res.data?.item1;
    });
  }

  getAllDeliveryNoteStatuses() {
    this.enumValueService.getDeliveryNoteStatuses().subscribe((res) => {
      console.log("rq status:", res);
      this.deliveryNoteStatuses = res;
    });
  }

  getDeliveryNoteStatus(value) {
    return this.statusColorService.getDeliveryNoteStatus(value);
  }

  getDeliveryNoteName(value: number) {
    return DeliveryNoteStatus[value];
  }

  getDeliveryNotes(): void {
    this.deliveryNoteRequest.deliveryNoteStatuses = [4];
    //this.deliveryNoteRequest.keyword = keyword;
    //this.deliveryNoteRequest.rowsPerPage=5;
    this.deliveryNoteService
      .getDeliveryNotes(this.deliveryNoteRequest)
      .subscribe((res) => {
        this.dataSource = new MatTableDataSource<DeliveryNoteResponseDTO>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  getDeliveryNotesNo(keyword): void {
    this.deliveryNoteRequest.keyword = keyword;
    this.deliveryNoteRequest.deliveryNoteStatuses = [4];
    this.deliveryNoteService
      .getDeliveryNotes(this.deliveryNoteRequest)
      .subscribe((res) => {
        this.deliveryNotes = res?.data?.item1;
      });
  }

  onDeliveryNoteNoChange(): void {
    const term = this.searchForm.get("deliveryNoteNo");
    this._filterProduct(term.value || "");
  }

  private _filterProduct(value: string) {
    const filterValue = value.toLowerCase().trim();
    this.getDeliveryNotesNo(filterValue);
  }

  onSearch() {
    this.loading = true;
    console.log(this.searchForm.value);
    this.deliveryNoteRequest.keyword = this.searchForm.value.deliveryNoteNo;
    this.deliveryNoteRequest.customerId = this.searchForm.value.customerId;
    //let deliveryNoteNo = this.searchForm.get("deliveryNoteNo").value;
    this.getDeliveryNotes();
  }

  onPageChange(pageEvent) {
    this.deliveryNoteRequest.page = pageEvent.pageIndex;
    this.deliveryNoteRequest.rowsPerPage = pageEvent.pageSize;
    this.getDeliveryNotes();
  }

  handleRadioChange($event) {
    this.selectedDeliveryNote = $event.value;
  }
}
