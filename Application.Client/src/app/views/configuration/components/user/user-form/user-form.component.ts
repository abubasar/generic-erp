import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { RoleRequest } from "app/views/configuration/models/role/role-request.model";
import { Role } from "app/views/configuration/models/role/role.model";
import { User } from "app/views/configuration/models/user/user.model";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { RoleService } from "app/views/configuration/services/role.service";
import { UserService } from "app/views/configuration/services/user.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-user-form",
  templateUrl: "./user-form.component.html",
  styleUrls: ["./user-form.component.scss"],
})
export class UserFormComponent implements OnInit {
  formTitle: string;
  userForm: FormGroup;
  employees: Employee[];
  filterEmployees: Employee[];
  roles: Role[];
  employee:Employee;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: User,
    private roleService: RoleService,
    private userService: UserService,
    private employeeService: EmployeeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllEmployees();
    this.getAllRoles();
  }

  initializeForm() {
    this.userForm = this.fb.group({
      id: [this.data?.id ?? null],
      employeeId: [this.data?.employeeId, Validators.required],
      username: [this.data?.username, Validators.required],
      roleId: [this.data?.roleId, Validators.required],
      password: [
        this.data?.password,
        [
          Validators.pattern(
            `^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$`
          ),
        ],
      ],
    });
    if (
      this.userForm.get("id").value === "" ||
      this.userForm.get("id").value == null
    ) {
      this.formTitle = "Add User";
    } else {
      this.formTitle = "Edit User";
    }
  }

  getAllRoles(): void {
    let roleRequest = new RoleRequest();
    roleRequest.page = -1;
    this.roleService.getRoles(roleRequest).subscribe((res) => {
      this.roles = res?.data?.item1;
    });
  }

  /** -----------------Start Autocomplete------------------ */
  onControlEmployeeChange(event: any): void {
    const name = event.target?.name;
    if (name === "employeeId") {
      const term = this.userForm.get("employeeId");
      this.filterEmployee(term.value || "");
    }
  }

  private filterEmployee(value: string) {
    const filterValue = value.toLowerCase();
    this.filterEmployees = this.employees?.filter((option) => {
      const fullName = `${option.firstName}  ${option.lastName}`;
      return fullName.toLowerCase().includes(filterValue);
    });
  }

  getEmployeeName(employeeId: string) {
    if (!employeeId) {
      return;
    }
    const employee =
      this.employees?.find((employee) => employee?.id === employeeId) ||
      this.data?.employee;
    return employee?.fullName;
    
  }

  getAllEmployees() {
    this.employeeService.getAllEmployees().subscribe((res) => {
      this.filterEmployees = this.employees = res?.data?.item1;
    });
  }
  /** ------------------End Autocomplete------------- */

  addUser(body): void {
    this.userService.createUser(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  updateUser(body): void {
    this.userService.updateUser(body).subscribe((res) => {
      if (res?.succeeded) {
        this.toastr.success(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }

  onSubmit() {
    if (this.userForm.value) {
      if (
        this.userForm.get("id").value === "" ||
        this.userForm.get("id").value == null
      ) {
        this.addUser(this.userForm.value);
      } else {
        this.updateUser(this.userForm.value);
      }
    }
  }
}
