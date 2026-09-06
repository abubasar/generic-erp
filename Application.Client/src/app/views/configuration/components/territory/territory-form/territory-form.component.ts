import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";
import { Territory } from "app/views/configuration/models/Territory/territory.model";
import { AreaService } from "app/views/configuration/services/area.service";
import { TerritoryService } from "app/views/configuration/services/territory.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-territory-form",
  templateUrl: "./territory-form.component.html",
  styleUrls: ["./territory-form.component.scss"],
})
export class TerritoryFormComponent implements OnInit {
  formTitle: string;
  territoryForm: FormGroup;
  areas: Area[];

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: Territory,
    private territoryService: TerritoryService,
    private areaService: AreaService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getAllAreas();
    this.initializeForm();
  }
  initializeForm() {
    this.territoryForm = this.fb.group({
      id: [this.data?.id ?? null],
      name: [this.data?.name, Validators.required],
      areaId: [this.data?.areaId, Validators.required],
    });
    if (
      this.territoryForm.get("id").value === "" ||
      this.territoryForm.get("id").value == null
    ) {
      this.formTitle = "Add Territory";
    } else {
      this.formTitle = "Edit Territory";
    }
  }

  getAllAreas(): void {
    let areaRequest = new AreaRequest();
    areaRequest.page = -1;
    this.areaService.getAreas(areaRequest).subscribe((res) => {
      this.areas = res?.data?.item1;
    });
  }

  addTerritory(body): void {
    this.territoryService.createTerritory(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateTerritory(body): void {
    this.territoryService.updateTerritory(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.territoryForm.value) {
      if (
        this.territoryForm.get("id").value === "" ||
        this.territoryForm.get("id").value == null
      ) {
        this.addTerritory(this.territoryForm.value);
      } else {
        this.updateTerritory(this.territoryForm.value);
      }
    }
  }
}
