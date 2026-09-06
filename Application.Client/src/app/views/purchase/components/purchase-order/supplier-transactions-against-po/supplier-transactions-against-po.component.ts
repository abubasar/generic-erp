import { Component, Inject, OnInit } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { PurchaseOrderResponseDTO } from "app/views/purchase/models/purchase-order/purchase-order-response-dto.model";
import { SupplierTransactionsAgainstPOResponseDTO } from "app/views/purchase/models/purchase-order/supplier-transactions-against-po-response-dto.model";
import { PurchaseOrderService } from "app/views/purchase/services/purchase-order.service";

@Component({
  selector: "app-supplier-transactions-against-po",
  templateUrl: "./supplier-transactions-against-po.component.html",
  styleUrls: ["./supplier-transactions-against-po.component.scss"],
})
export class SupplierTransactionsAgainstPOComponent implements OnInit {
  loading: boolean = true;
  displayedColumns: string[] = [
    "transactionDate",
    "ponumber",
    "supplierName",
    "supplierTransactionTypeName",
    "supplierInvoiceDate",
    "paymentTermInDays",
    "amount",
    "balance",
    "remark",
  ];
  dataSource: MatTableDataSource<SupplierTransactionsAgainstPOResponseDTO>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PurchaseOrderResponseDTO,
    private dialogRef: MatDialogRef<SupplierTransactionsAgainstPOComponent>,
    private purchaseOrderService: PurchaseOrderService
  ) {}

  ngOnInit(): void {
    this.getSupplierTransactionsAgainstPO(this.data?.id);
  }

  getSupplierTransactionsAgainstPO(id) {
    this.purchaseOrderService
      .getSupplierTransactionsAgainstPO(id)
      .subscribe((res) => {
        console.log(res);
        this.dataSource =
          new MatTableDataSource<SupplierTransactionsAgainstPOResponseDTO>(
            res?.data
          );
        this.loading = false;
      });
  }
}
