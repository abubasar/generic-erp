import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";

@Injectable({
  providedIn: "root",
})
export class SnackBarService {
  constructor(private snackBar: MatSnackBar) {}

  ngOnInit() {}

  openSnackbar(message: string) {
    this.snackBar.open(message, "Close", {
      duration: 5000,
      horizontalPosition: "right",
      verticalPosition: "top",
    });
  }
}
