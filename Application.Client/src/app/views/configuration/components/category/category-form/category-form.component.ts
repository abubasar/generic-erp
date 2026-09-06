import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Category } from "app/views/configuration/models/category/category.model";
import { CategoryService } from "app/views/configuration/services/category.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-category-form",
  templateUrl: "./category-form.component.html",
  styleUrls: ["./category-form.component.scss"],
})
export class CategoryFormComponent implements OnInit {
  formTitle: string;
  categoryForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Category,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.categoryForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.categoryForm.get("id").value === "" ||
      this.categoryForm.get("id").value == null
    ) {
      this.formTitle = "Add Category";
    } else {
      this.formTitle = "Edit Category";
    }
  }

  addCategory(body): void {
    this.categoryService.createCategory(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateCategory(body): void {
    this.categoryService.updateCategory(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.categoryForm.value) {
      if (
        this.categoryForm.get("id").value === "" ||
        this.categoryForm.get("id").value == null
      ) {
        this.addCategory(this.categoryForm.value);
      } else {
        this.updateCategory(this.categoryForm.value);
      }
    }
  }
}
