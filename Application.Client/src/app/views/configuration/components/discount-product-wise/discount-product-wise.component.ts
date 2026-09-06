import {
  animate,
  state,
  style,
  transition,
  trigger,
} from "@angular/animations";
import { Component, OnInit, ViewChild } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatMenuTrigger } from "@angular/material/menu";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { DateTimeFormatService } from "app/shared/services/date-time-format.service";
import { ToastrService } from "ngx-toastr";

import { HttpClient } from "@angular/common/http";
import { Observable, merge, of } from "rxjs";
import { DiscountProductWiseRequest } from "../../models/discount-product-wise/discount-product-wise-request.model";
import { DiscountProductWise } from "../../models/discount-product-wise/discount-product-wise.model";
import { DiscountProductWiseService } from "../../services/discount-product-wise.service";
import { DiscountProductWiseAddFormComponent } from "./discount-product-wise-add-form/discount-product-wise-add-form.component";
import { DiscountProductWiseEditFormComponent } from "./discount-product-wise-edit-form/discount-product-wise-edit-form.component";

@Component({
  selector: "app-discount-product-wise",
  templateUrl: "./discount-product-wise.component.html",
  styleUrls: ["./discount-product-wise.component.scss"],
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
export class DiscountProductWiseComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  // customers: Customer[];
  // filterCustomers: Customer[];
  viewColumnForm: FormGroup;
  expandedElement: any;
  dataSource: MatTableDataSource<DiscountProductWise>;
  totalCount: number;
  discountProductWiseRequest = new DiscountProductWiseRequest();
  displayedColumns$: Observable<string[]>;
  pageSizeOptions: [] = Page_Size_Options;

  @ViewChild(MatMenuTrigger) trigger: MatMenuTrigger;

  //  * Control column ordering and which columns are displayed.
  columnDefinitions = [
    { def: "expand", label: "Expand" },
    { def: "actions", label: "Actions" },
    { def: "name", label: "Offer Name" },
    { def: "startDate", label: "Start Date" },
    { def: "endDate", label: "End Date" },
    { def: "isActive", label: "Is Active" },
  ];

  constructor(
    private discountProductWiseService: DiscountProductWiseService,
    public dateFormatService: DateTimeFormatService,
    // private customerService: CustomerService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    // this.getAllCustomers();
    this.getDiscountProductWises(this.discountProductWiseRequest);
    this.subscribeToFormChanges();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      startDate: [null],
      endDate: [null],
      // customerId: [null],
    });
    this.viewColumnForm = this.fb.group({
      expand: [true],
      name: [true],
      startDate: [true],
      endDate: [true],
      isActive: [true],
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

  collapsed: string = "collapsed";
  expanded: string = "expanded";

  expandAll() {
    if (this.collapsed === "collapsed") this.collapsed = "expanded";
    else this.collapsed = "collapsed";
  }

  // handleCustomerSearch(event: any): void {
  //   const name = event.target?.name;
  //   if (name === "customerId") {
  //     const term = this.searchForm.get("customerId");
  //     this.filterCustomer(term.value || "");
  //   }
  // }

  // private filterCustomer(value: string) {
  //   const filterValue = value.trim().toLowerCase();
  //   this.filterCustomers = this.customers?.filter((option) =>
  //     option.name.toLowerCase().includes(filterValue) ||
  //     option.code?.slice(-4).toLowerCase().includes(filterValue)
  //   );
  // }

  // getCustomerName(customerId: string) {
  //   if (!customerId) {
  //     return;
  //   }
  //   const customerAccount = this.customers?.find(
  //     (customer) => customer?.id === customerId
  //   );
  //   return customerAccount?.name;
  // }

  // getAllCustomers() {
  //   this.customerService.getAllCustomers().subscribe((res) => {
  //     this.customers = res.data?.item1;
  //   });
  // }

  getDiscountProductWises(request: DiscountProductWiseRequest): void {
    this.discountProductWiseService
      .getDiscountProductWises(request)
      .subscribe((res) => {
        console.log("Discount Product Wise", res);
        this.dataSource = new MatTableDataSource<DiscountProductWise>(
          res?.data?.item1
        );
        this.totalCount = res?.data?.item2;
        this.loading = false;
      });
  }

  remove(id): void {
    this.discountProductWiseService
      .deleteDiscountProductWise(id)
      .subscribe((res) => {
        if (res?.succeeded) {
          this.getDiscountProductWises(this.discountProductWiseRequest);
          this.toastr.info(res?.message);
        } else {
          this.toastr.error(res?.message);
        }
      });
  }

  openAddNewForm() {
    const dialogRef = this.dialog.open(DiscountProductWiseAddFormComponent, {
      disableClose: true,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getDiscountProductWises(this.discountProductWiseRequest);
      }
    });
  }

  openEditForm(discountProductWise?: DiscountProductWise): void {
    const dialogRef = this.dialog.open(DiscountProductWiseEditFormComponent, {
      disableClose: true,
      data: discountProductWise,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getDiscountProductWises(this.discountProductWiseRequest);
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
    this.discountProductWiseRequest = new DiscountProductWiseRequest();
    this.getDiscountProductWises(this.discountProductWiseRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateDiscountRequest();
    console.log(this.discountProductWiseRequest);
    this.getDiscountProductWises(this.discountProductWiseRequest);
  }

  onPageChange(pageEvent) {
    this.updateDiscountRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getDiscountProductWises(this.discountProductWiseRequest);
  }

  private updateDiscountRequest(
    pageIndex = this.discountProductWiseRequest.page,
    pageSize = this.discountProductWiseRequest.rowsPerPage
  ) {
    this.discountProductWiseRequest = {
      ...this.discountProductWiseRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  // printPdf(id) {
  //   this.http
  //     .get(environment.apiURL + "/DiscountProductWises/" + id, {
  //       responseType: "blob",
  //     })
  //     .subscribe((response) => {
  //       //Create a Blob from the PDF Stream
  //       const file = new Blob([response], { type: "application/pdf" });
  //       //Build a URL from the file
  //       const fileURL = URL.createObjectURL(file);
  //       //Open the URL on new Window
  //       const pdfWindow = window.open();
  //       pdfWindow.location.href = fileURL;
  //     });
  // }
}
