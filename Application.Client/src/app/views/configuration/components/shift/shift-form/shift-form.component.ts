import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { Shift } from "app/views/configuration/models/shift/shift.model";
import { ShiftService } from "app/views/configuration/services/shift.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-shift-form",
  templateUrl: "./shift-form.component.html",
  styleUrls: ["./shift-form.component.scss"],
})
export class ShiftFormComponent implements OnInit {
  formTitle: string;
  shiftForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Shift,
    private dialog: MatDialog,
    private shiftService: ShiftService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.shiftForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      fromTime: [this.data?.fromTime, Validators.required],
      toTime: [this.data?.toTime, Validators.required],
    });

    if (
      this.shiftForm.get("id").value === "" ||
      this.shiftForm.get("id").value == null
    ) {
      this.formTitle = "Add Shift";
    } else {
      this.formTitle = "Edit Shift";
    }
  }

  addShift(body): void {
    console.log("Test-------", body);
    this.shiftService.createShift(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateShift(body): void {
    this.shiftService.updateShift(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
        this.dialog.closeAll();
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.shiftForm.value) {
      console.log(this.shiftForm.value);
      if (
        this.shiftForm.get("id").value === "" ||
        this.shiftForm.get("id").value == null
      ) {
        this.addShift(this.shiftForm.value);
      } else {
        this.updateShift(this.shiftForm.value);
      }
    }
  }
}
