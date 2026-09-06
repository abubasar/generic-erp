import { Component, Inject, OnInit } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { ConfirmReadyForGrnDialogModel } from "app/shared/models/confirm-ready-for-grn-dialog.model";
import { LCCostEntryByPoIdResponseDTO } from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-by-po-id-response-dto.model";
import { LcCostEntryService } from "app/views/purchase/services/lc-cost-entry.service";

@Component({
  selector: "app-confirm-ready-for-grn-dialog",
  templateUrl: "./confirm-ready-for-grn-dialog.component.html",
  styleUrls: ["./confirm-ready-for-grn-dialog.component.scss"],
})
export class ConfirmReadyForGrnDialogComponent implements OnInit {
  id: string;
  title: string;
  message: string;
  dataSource: LCCostEntryByPoIdResponseDTO[];

  displayedColumns: string[] = [
    "lcCostEntryNo",
    "debitAccountName",
    "creditAccountName",
    "amount",
    "isIncludedWithinLandedCost",
  ];

  constructor(
    public lcCostEntryService: LcCostEntryService,
    public dialogRef: MatDialogRef<ConfirmReadyForGrnDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmReadyForGrnDialogModel
  ) {
    // Update view with given values
    this.id = data.id;
    this.title = data.title;
    this.message = data.message;
  }

  ngOnInit() {
    this.getLcCostEntriesByPo();
  }

  getLcCostEntriesByPo(): void {
    this.lcCostEntryService
      .getLcCostEntryByPurchaseOrderId(this.id)
      .subscribe((res) => {
        this.dataSource = res.data;
      });
  }

  onConfirm(): void {
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }
}
