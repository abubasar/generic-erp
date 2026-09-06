import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { RFQSentSupplierResponseDTO } from "app/views/purchase/models/purchase-requisition/rfq-sent-supplier-response-dto.model";
import { PurchaseRequisitionService } from "app/views/purchase/services/purchase-requisition.service";

@Component({
  selector: "app-rfq-sent-supplier",
  templateUrl: "./rfq-sent-supplier.component.html",
  styleUrls: ["./rfq-sent-supplier.component.scss"],
})
export class RfqSentSupplierComponent implements OnInit {
  loading:boolean=true;
  displayedColumns: string[] = ["name", "contactNo", "email"];
  dataSource: MatTableDataSource<RFQSentSupplierResponseDTO>;
  // dataSource: MatTableDataSource<{
  //   name: string;
  //   contactNo: string;
  //   email: string;
  // }>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PurchaseRequisitionResponseDTO,
    private dialogRef: MatDialogRef<RfqSentSupplierComponent>,
    private purchaseRequisitionService: PurchaseRequisitionService
  ) {}

  ngOnInit(): void {
    this.getRfqSentSuppliers(this.data?.id);
  }

  getRfqSentSuppliers(id) {
    this.purchaseRequisitionService
      .getRfqSentSuppliers(id)
      .subscribe((res) => {
        console.log(res);
        this.dataSource = new MatTableDataSource<RFQSentSupplierResponseDTO>(
          res?.data
        );
        this.loading = false;
        // this.totalCount = res?.data?.item2;
      });
  }
}

// <table mat-table [dataSource]="dataSource" class="mat-elevation-z8">
//   <!-- name Column -->
//   <ng-container matColumnDef="name">
//     <th mat-header-cell *matHeaderCellDef>Name</th>
//     <td mat-cell *matCellDef="let element">{{ element.name }}</td>
//   </ng-container>

//   <!-- contactNo Column -->
//   <ng-container matColumnDef="contactNo">
//     <th mat-header-cell *matHeaderCellDef>Contact No</th>
//     <td mat-cell *matCellDef="let element">{{ element.contactNo }}</td>
//   </ng-container>

//   <!-- email Column -->
//   <ng-container matColumnDef="email">
//     <th mat-header-cell *matHeaderCellDef>Email</th>
//     <td mat-cell *matCellDef="let element">{{ element.email }}</td>
//   </ng-container>

//   <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
//   <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
// </table>
