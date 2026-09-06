// loading-skeleton.component.ts
import { Component, Input } from "@angular/core";

@Component({
  selector: "app-loading-skeleton",
  templateUrl: "./loading-skeleton.component.html",
  styleUrls: ["./loading-skeleton.component.scss"],
})
export class LoadingSkeletonComponent {
  @Input() skeletonRowsCount = 10; // set to the number of rows you want to display
}
