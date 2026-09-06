import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { DeliveryPlace } from "app/views/configuration/models/delivery-place/delivery-place.model";
import { DeliveryPlaceService } from "app/views/configuration/services/delivery-place.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-delivery-place-form",
  templateUrl: "./delivery-place-form.component.html",
  styleUrls: ["./delivery-place-form.component.scss"],
})
export class DeliveryPlaceFormComponent implements OnInit {
  formTitle: string;
  deliveryPlaceForm: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: DeliveryPlace,
    private deliveryPlaceService: DeliveryPlaceService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.deliveryPlaceForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
    });
    if (
      this.deliveryPlaceForm.get("id").value === "" ||
      this.deliveryPlaceForm.get("id").value == null
    ) {
      this.formTitle = "Add Delivery Place";
    } else {
      this.formTitle = "Edit Delivery Place";
    }
  }

  addDeliveryPlace(body): void {
    this.deliveryPlaceService.createDeliveryPlace(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateDeliveryPlace(body): void {
    this.deliveryPlaceService.updateDeliveryPlace(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.deliveryPlaceForm.value) {
      if (
        this.deliveryPlaceForm.get("id").value === "" ||
        this.deliveryPlaceForm.get("id").value == null
      ) {
        this.addDeliveryPlace(this.deliveryPlaceForm.value);
      } else {
        this.updateDeliveryPlace(this.deliveryPlaceForm.value);
      }
    }
  }
}
