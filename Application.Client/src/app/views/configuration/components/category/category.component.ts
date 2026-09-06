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
import { CategoryRequest } from "../../models/category/category-request.model";
import { Category } from "../../models/category/category.model";
import { CategoryService } from "../../services/category.service";
import { CategoryFormComponent } from "./category-form/category-form.component";

@Component({
  selector: "app-category",
  templateUrl: "./category.component.html",
  styleUrls: ["./category.component.scss"],
})
export class CategoryComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = ["actions", "name"]; //"code",
  dataSource: MatTableDataSource<Category>;
  totalCount: number;
  categoryRequest = new CategoryRequest();
  pageSizeOptions: [] = Page_Size_Options;

  constructor(
    private categoryService: CategoryService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getCategories(this.categoryRequest);
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getCategories(request: CategoryRequest): void {
    this.categoryService.getCategories(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Category>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.categoryService.deleteCategory(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getCategories(this.categoryRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  openForm(category?: Category): void {
    const dialogRef = this.dialog.open(CategoryFormComponent, {
      disableClose: true,
      data: category,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getCategories(this.categoryRequest);
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
    this.categoryRequest = new CategoryRequest();
    this.getCategories(this.categoryRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateCategoryRequest();
    this.getCategories(this.categoryRequest);
  }

  onPageChange(pageEvent) {
    this.updateCategoryRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getCategories(this.categoryRequest);
  }

  private updateCategoryRequest(
    pageIndex = this.categoryRequest.page,
    pageSize = this.categoryRequest.rowsPerPage
  ) {
    this.categoryRequest = {
      ...this.categoryRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Category/print", this.searchForm.value, {
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
