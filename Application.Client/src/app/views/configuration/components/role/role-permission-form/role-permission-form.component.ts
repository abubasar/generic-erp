import { Component, Inject, OnInit } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialog } from "@angular/material/dialog";
import { RoleClaim } from "app/views/configuration/models/role/permission.model";
import { Role } from "app/views/configuration/models/role/role.model";
import { RoleService } from "app/views/configuration/services/role.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-role-permission-form",
  templateUrl: "./role-permission-form.component.html",
  styleUrls: ["./role-permission-form.component.scss"],
})
export class RolePermissionFormComponent implements OnInit {
  rolePermissionColumns: String[] = [
    "type",
    "group",
    "value",
    "description",
    "selected",
  ];
  rolePermission;
  rolePermissionGroup: string[];

  groupRoleClaims: Record<string, RoleClaim[]> = {};

  // selection = new SelectionModel<RoleClaim>(true, []);

  /** Whether the number of selected elements matches the total number of rows. */
  // isAllSelected(group) {
  //   const numSelected = this.selection.selected.length;
  //   // console.log(this.groupRoleClaims[group]);
  //   const numRows = this.groupRoleClaims[group].length;
  //   console.log("numSelected:", numSelected, "numRows:", numRows);
  //   return numSelected === numRows;
  // }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  // toggleAllRows(group) {
  //   console.log(group);
  //   console.log(this.groupRoleClaims[group]);
  //   this.groupRoleClaims[group].forEach((claim) => {
  //     claim.selected = !claim.selected;
  //     console.log(claim.selected);
  //   });

  //   console.log(this.isAllSelected(group));

  //   // if (this.isAllSelected()) {
  //   //   this.selection.clear();
  //   //   return;
  //   // }
  //   // this.selection.select(...this.dataSource.data);
  // }

  // toggleRow(row: RoleClaim) {
  //   this.selection.toggle(row);
  //   console.log(this.selection);
  // }

  // /** The label for the checkbox on the passed row */
  // checkboxLabel(row?: PeriodicElement): string {
  //   if (!row) {
  //     return `${this.isAllSelected() ? "deselect" : "select"} all`;
  //   }
  //   return `${this.selection.isSelected(row) ? "deselect" : "select"} row ${
  //     row.position + 1
  //   }`;
  // }

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: Role,
    private dialogRef: MatDialog,
    public roleService: RoleService,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getPermissions();
  }

  getPermissions(): void {
    this.roleService
      .getRolePermissionsByRoleId(this.data.id)
      .subscribe((result) => {
        this.rolePermission = result?.data;
        console.log(this.rolePermission);
        this.rolePermissionGroup = [
          ...new Set(result?.data.map((item) => item.group)),
        ];
        console.log(this.rolePermissionGroup);
        console.log(this.groupRoleClaims);
        this.rolePermission.forEach((claim) => {
          if (
            Object.keys(this.groupRoleClaims).find((key) => key === claim.group)
          ) {
            this.groupRoleClaims[claim.group].push(claim);
          } else {
            this.groupRoleClaims[claim.group] = [claim];
          }
        });
      });
  }

  getSelectedCount(roleClaims: RoleClaim[]): number {
    return roleClaims?.filter((claim) => claim.selected).length;
  }

  getGroupBadgeColor(selected: number, all: number): string {
    if (selected == 0) return "warn";
    if (selected == all) return "accent";
    return "primary";
  }

  submitRolePermission(): void {
    var selectedRoleClaims = [];
    Object.entries(this.groupRoleClaims).forEach(([key, value]) => {
      value.forEach((claim) => {
        if (claim.selected) {
          selectedRoleClaims.push(claim);
        }
      });
    });
    this.roleService
      .updateRolePermissions({
        roleId: this.data.id,
        roleClaims: selectedRoleClaims,
      })
      .subscribe((result) => {
        this.toastr.success(result.message);
        this.dialogRef.closeAll();
      });
  }
}
