import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { RegionRequest } from "app/views/configuration/models/region/region-request.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { RegionService } from "app/views/configuration/services/region.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-zone-form",
  templateUrl: "./zone-form.component.html",
  styleUrls: ["./zone-form.component.scss"],
})
export class ZoneFormComponent implements OnInit {
  formTitle: string;
  zoneForm: FormGroup;
  regions: Region[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Zone,
    private zoneService: ZoneService,
    private regionService: RegionService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllRegions();
    this.initializeForm();
  }
  initializeForm() {
    this.zoneForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      regionId: [this.data?.regionId, Validators.required],
    });
    if (
      this.zoneForm.get("id").value === "" ||
      this.zoneForm.get("id").value == null
    ) {
      this.formTitle = "Add Zone";
    } else {
      this.formTitle = "Edit Zone";
    }
  }

  getAllRegions(): void {
    let regionRequest = new RegionRequest();
    regionRequest.page = -1;
    this.regionService.getRegions(regionRequest).subscribe((res) => {
      console.log(res?.data?.item1);
      this.regions = res?.data?.item1;
    });
  }

  addZone(body): void {
    this.zoneService.createZone(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateZone(body): void {
    this.zoneService.updateZone(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.zoneForm.value) {
      if (
        this.zoneForm.get("id").value === "" ||
        this.zoneForm.get("id").value == null
      ) {
        this.addZone(this.zoneForm.value);
      } else {
        this.updateZone(this.zoneForm.value);
      }
    }
  }
}
