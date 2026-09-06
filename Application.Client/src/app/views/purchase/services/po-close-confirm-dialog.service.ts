import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ConfirmDialogComponent } from "app/shared/components/confirm-dialog/confirm-dialog.component";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";

@Injectable({
  providedIn: "root",
})
export class PoCloseConfirmDialogService {
  constructor(private dialog: MatDialog) {}

  /**
   * Confirmation Dialog Function
   * @param id - The ID of the item to be confirmed for deletion
   * @param callBack - The callback function to be executed if the confirmation is true
   * @param title - The title of the confirmation dialog (default: "Confirm Action")
   * @param message - The message in the confirmation dialog (default: "Are you sure you want to do this?")
   * @param index - The Index is a number where the task will be executed
   */
  confirmDialog(
    id: string,
    callBack: Function,
    message: string = `Are you sure you want to do this?`,
    title: string = "Confirm Action",
    index: number = -1
  ): MatDialogRef<ConfirmDialogComponent> {
    const dialogData: ConfirmDialogModel = {
      title: title,
      message: message,
    };

    // Open the confirmation dialog with the specified title and message
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      disableClose: true,
      data: dialogData,
      position: {
        top: "0", // Set the top position to 0
      },
    });

    // Subscribe to the dialog close event
    dialogRef.afterClosed().subscribe((dialogResult: boolean) => {
      if (dialogResult && index != -1) {
        // Call the provided callback function with the ID if the confirmation is true
        callBack(id, index);
      }
    });

    return dialogRef;
  }
}
