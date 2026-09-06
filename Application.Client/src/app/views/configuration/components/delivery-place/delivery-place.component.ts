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
import { DeliveryPlaceRequest } from "../../models/delivery-place/delivery-place-request.model";
import { DeliveryPlace } from "../../models/delivery-place/delivery-place.model";
import { DeliveryPlaceService } from "../../services/delivery-place.service";
import { DeliveryPlaceFormComponent } from "./delivery-place-form/delivery-place-form.component";

@Component({
  selector: "app-delivery-place",
  templateUrl: "./delivery-place.component.html",
  styleUrls: ["./delivery-place.component.scss"],
})
export class DeliveryPlaceComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"];
  dataSource: MatTableDataSource<DeliveryPlace>;
  totalCount: number;
  deliveryPlaceRequest = new DeliveryPlaceRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private deliveryPlaceService: DeliveryPlaceService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getDeliveryPlaces(this.deliveryPlaceRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getDeliveryPlaces(request: DeliveryPlaceRequest): void {
    this.deliveryPlaceService.getDeliveryPlaces(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<DeliveryPlace>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.deliveryPlaceService.deleteDeliveryPlace(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getDeliveryPlaces(this.deliveryPlaceRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(deliveryPlace?: DeliveryPlace): void {
    const dialogRef = this.dialog.open(DeliveryPlaceFormComponent, {
      disableClose: true,
      data: deliveryPlace,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getDeliveryPlaces(this.deliveryPlaceRequest);
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
    this.deliveryPlaceRequest = new DeliveryPlaceRequest();
    this.getDeliveryPlaces(this.deliveryPlaceRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateDeliveryPlaceRequest();
    this.getDeliveryPlaces(this.deliveryPlaceRequest);
  }

  onPageChange(pageEvent) {
    this.updateDeliveryPlaceRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getDeliveryPlaces(this.deliveryPlaceRequest);
  }

  private updateDeliveryPlaceRequest(
    pageIndex = this.deliveryPlaceRequest.page,
    pageSize = this.deliveryPlaceRequest.rowsPerPage
  ) {
    this.deliveryPlaceRequest = {
      ...this.deliveryPlaceRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(
        environment.apiURL + "/DeliveryPlace/print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
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
