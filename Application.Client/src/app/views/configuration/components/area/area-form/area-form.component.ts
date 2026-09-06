import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Area } from "app/views/configuration/models/area/area.model";
import { ZoneRequest } from "app/views/configuration/models/zone/zone-request.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-area-form",
  templateUrl: "./area-form.component.html",
  styleUrls: ["./area-form.component.scss"],
})
export class AreaFormComponent implements OnInit {
  formTitle: string;
  areaForm: FormGroup;
  zones: Zone[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Area,
    private areaService: AreaService,
    private zoneService: ZoneService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllZones();
    this.initializeForm();
  }
  initializeForm() {
    this.areaForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      zoneId: [this.data?.zoneId, Validators.required],
    });

    if (
      this.areaForm.get("id").value === "" ||
      this.areaForm.get("id").value == null
    ) {
      this.formTitle = "Add Area";
    } else {
      this.formTitle = "Edit Area";
    }
  }
  getAllZones(): void {
    let zoneRequest = new ZoneRequest();
    zoneRequest.page = -1;
    this.zoneService.getZones(zoneRequest).subscribe((res) => {
      console.log(res?.data?.item1);
      this.zones = res?.data?.item1;
    });
  }

  addArea(body): void {
    this.areaService.createArea(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateArea(body): void {
    this.areaService.updateArea(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.areaForm.value) {
      if (
        this.areaForm.get("id").value === "" ||
        this.areaForm.get("id").value == null
      ) {
        this.addArea(this.areaForm.value);
      } else {
        this.updateArea(this.areaForm.value);
      }
    }
  }
}
