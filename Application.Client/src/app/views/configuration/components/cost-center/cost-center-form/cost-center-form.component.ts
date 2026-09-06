import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-cost-center-form",
  templateUrl: "./cost-center-form.component.html",
  styleUrls: ["./cost-center-form.component.scss"],
})
export class CostCenterFormComponent implements OnInit {
  formTitle: string;
  costCenterForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: CostCenter,
    private costCenterService: CostCenterService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.costCenterForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.costCenterForm.get("id").value === "" ||
      this.costCenterForm.get("id").value == null
    ) {
      this.formTitle = "Add Cost Center";
    } else {
      this.formTitle = "Edit Cost Center";
    }
  }

  addCostCenter(body): void {
    this.costCenterService.createCostCenter(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateCostCenter(body): void {
    this.costCenterService.updateCostCenter(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.costCenterForm.value) {
      if (
        this.costCenterForm.get("id").value === "" ||
        this.costCenterForm.get("id").value == null
      ) {
        this.addCostCenter(this.costCenterForm.value);
      } else {
        this.updateCostCenter(this.costCenterForm.value);
      }
    }
  }
}
