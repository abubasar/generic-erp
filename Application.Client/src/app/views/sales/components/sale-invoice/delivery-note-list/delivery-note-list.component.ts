import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { SelectionModel } from "@angular/cdk/collections";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import { DeliveryNoteStatus } from "app/shared/enums/deliveryNoteStatus";
import { ENUM } from "app/shared/models/enum-value/enum.model";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { EnumValueService } from "app/shared/services/enum-value/enum-value.service";
import { SnackBarService } from "app/shared/services/snack-bar.service";
import { StatusColorService } from "app/shared/services/status-color/status-color.service";
import { DeliveryNoteResponseDTO } from "app/views/sales/models/delivery-note/delivery-note-response-dto.model";
import { DeliveryNoteSearchRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-search-request-dto.model";
import { DeliveryNoteService } from "app/views/sales/services/delivery-note.service";

@Component({
  selector: "app-delivery-note-list",
  templateUrl: "./delivery-note-list.component.html",
  styleUrls: ["./delivery-note-list.component.scss"],
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
export class DeliveryNoteListComponent implements OnInit {
  loading: boolean = true;
  selection = new SelectionModel<DeliveryNoteResponseDTO>(true, []); // its need for multiple selection
  deliveryNotes: DeliveryNoteResponseDTO[];
  deliveryNotesNo: DeliveryNoteResponseDTO[];
  customers: DeliveryNoteResponseDTO[];
  filterCustomers: DeliveryNoteResponseDTO[];
  deliveryNoteRequest = new DeliveryNoteSearchRequestDTO();

  panelOpenState = false;
  searchForm: FormGroup;
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
    private enumValueService: EnumValueService,
    private fb: FormBuilder,
    private snackBarService: SnackBarService
  ) {}

  ngOnInit(): void {
    this.getDeliveryNotes();
    this.getDeliveryNotesNo();
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
        this.deliveryNotes = this.deliveryNotesNo;
        this.searchForm
          .get("customerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  getAllDeliveryNoteStatuses() {
    this.enumValueService.getDeliveryNoteStatuses().subscribe((res) => {
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
    this.deliveryNoteRequest.page = -1;
    this.deliveryNoteRequest.deliveryNoteStatus = 3;
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

  getDeliveryNotesNo(): void {
    this.deliveryNoteRequest.page = -1;
    this.deliveryNoteRequest.deliveryNoteStatus = 3;
    this.deliveryNoteService
      .getDeliveryNotes(this.deliveryNoteRequest)
      .subscribe((res) => {
        const seen = new Set();
        const uniqueCustomersDN = res?.data?.item1.filter((deliveryNote) => {
          const isDuplicate = seen.has(deliveryNote.customerId);
          seen.add(deliveryNote.customerId);
          return !isDuplicate;
        });
        this.deliveryNotes = this.deliveryNotesNo = res?.data?.item1;
        this.filterCustomers = this.customers = uniqueCustomersDN;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "deliveryNoteNo") {
      this.searchForm?.get("deliveryNoteNo").setValue("");
    }
    if (fieldName === "customerId") {
      this.searchForm?.get("customerId").setValue(null);
      this.deliveryNotes = this.deliveryNotesNo;
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
        option?.customer?.name.toLowerCase().includes(filterValue) ||
        option?.customer?.code?.slice(-4).toLowerCase().includes(filterValue)
    );
  }

  getCustomerName(customerId: string) {
    if (!customerId) {
      return;
    }
    const customerAccount = this.customers?.find(
      (customer) => customer?.customerId === customerId
    );
    return customerAccount?.customer?.name;
  }

  onSelectedCustomerId(customerId: string) {
    this.searchForm.patchValue({
      saleOrderNo: "",
    });
    if (this.deliveryNotes) {
      this.deliveryNotes = this.deliveryNotesNo?.filter(
        (x) => x.customerId === customerId
      );
    }
  }

  onDeliveryNoteNoChange(): void {
    const term = this.searchForm.get("deliveryNoteNo");
    this._filterDeliveryNote(term.value || "");
  }

  private _filterDeliveryNote(value: string) {
    const filterValue = value.toLowerCase().trim();
    const customerId = this.searchForm.value.customerId;
    if (customerId) {
      this.deliveryNotes = this.deliveryNotesNo?.filter(
        (x) =>
          x.deliveryNoteNo.toLocaleLowerCase().includes(filterValue) &&
          x.customerId === customerId
      );
    } else {
      this.deliveryNotes = this.deliveryNotesNo?.filter((x) =>
        x.deliveryNoteNo.toLocaleLowerCase().includes(filterValue)
      );
    }
  }

  onSearch() {
    this.loading = true;
    this.deliveryNoteRequest = {
      ...this.deliveryNoteRequest,
      ...this.searchForm.value,
    };
    this.getDeliveryNotes();
  }

  isDuplicateSONumber: boolean = false;
  checkUniqueSONumber() {
    const selectedSONumbers = this.selection.selected.map(
      (note) => note.saleOrderNo
    );
    const uniqueSONumbers = new Set(selectedSONumbers);
    if (uniqueSONumbers.size > 1) {
      // Display alert message
      this.showSnackBar(
        "You cannot select multiple different Sale Order numbers at a time"
      );
      this.isDuplicateSONumber = true;
    } else this.isDuplicateSONumber = false;
  }

  showSnackBar(message: string) {
    this.snackBarService.openSnackbar(message);
  }
}
