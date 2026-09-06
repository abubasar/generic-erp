import { AfterViewInit, Component, Inject, OnInit, ViewChild } from "@angular/core";
import { MatButton } from "@angular/material/button";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";

@Component({
  selector: "app-confirm-dialog",
  templateUrl: "./confirm-dialog.component.html",
  styleUrls: ["./confirm-dialog.component.scss"],
})
export class ConfirmDialogComponent implements OnInit, AfterViewInit {
  @ViewChild("btnCancel") btnCancel: MatButton;
  title: string;
  message: string;
  isConfirmDisabled: boolean = false;
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogModel
  ) {
    // Update view with given values
    this.title = data.title;
    this.message = data.message;
  }
  ngAfterViewInit(): void {
    this.btnCancel.focus();
  }

  ngOnInit() {}

  onConfirm(): void {
    this.isConfirmDisabled = true;
    // Close the dialog, return true
    this.dialogRef.close(true);
  }

  onDismiss(): void {
    // Close the dialog, return false
    this.dialogRef.close(false);
  }
}
