import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ConfirmReadyForGrnDialogComponent } from "../components/confirm-ready-for-grn-dialog/confirm-ready-for-grn-dialog.component";
import { ConfirmReadyForGrnDialogModel } from "../models/confirm-ready-for-grn-dialog.model";

@Injectable({
  providedIn: "root",
})
export class ConfirmReadyForGrnDialogService {
  constructor(private dialog: MatDialog) {}

  /**
   * Confirmation Dialog Function
   * @param id - The ID of the item to be confirmed for deletion
   * @param callBack - The callback function to be executed if the confirmation is true
   * @param title - The title of the confirmation dialog (default: "Confirm Action")
   * @param message - The message in the confirmation dialog (default: "Are you sure you want to do this?")
   */
  confirmDialog(
    id: string,
    callBack: Function,
    message: string = `Are you sure you want to do this?`,
    title: string = "Confirm Action"
  ): MatDialogRef<ConfirmReadyForGrnDialogComponent> {
    const dialogData: ConfirmReadyForGrnDialogModel = {
      id: id,
      title: title,
      message: message,
    };

    // Open the confirmation dialog with the specified title and message
    const dialogRef = this.dialog.open(ConfirmReadyForGrnDialogComponent, {
      disableClose: true,
      data: dialogData,
      panelClass: "add-bill-container",
      minHeight: "auto",
      height: "auto",
      position: {
        top: "0", // Set the top position to 0
      },
    });

    // Subscribe to the dialog close event
    dialogRef.afterClosed().subscribe((dialogResult: boolean) => {
      if (dialogResult) {
        // Call the provided callback function with the ID if the confirmation is true
        callBack(id);
      }
    });

    return dialogRef;
  }
}
