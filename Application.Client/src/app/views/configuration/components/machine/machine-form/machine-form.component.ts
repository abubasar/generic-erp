import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Machine } from "app/views/configuration/models/machine/machine.model";
import { MachineService } from "app/views/configuration/services/machine.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-machine-form",
  templateUrl: "./machine-form.component.html",
  styleUrls: ["./machine-form.component.scss"],
})
export class MachineFormComponent implements OnInit {
  formTitle: string;
  machineForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Machine,
    private machineService: MachineService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.machineForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.machineForm.get("id").value === "" ||
      this.machineForm.get("id").value == null
    ) {
      this.formTitle = "Add Machine";
    } else {
      this.formTitle = "Edit Machine";
    }
  }

  addMachine(body): void {
    this.machineService.createMachine(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateMachine(body): void {
    this.machineService.updateMachine(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.machineForm.value) {
      if (
        this.machineForm.get("id").value === "" ||
        this.machineForm.get("id").value == null
      ) {
        this.addMachine(this.machineForm.value);
      } else {
        this.updateMachine(this.machineForm.value);
      }
    }
  }
}
