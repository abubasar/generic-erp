// loading-skeleton.component.ts
import { Component, Input, ChangeDetectionStrategy } from "@angular/core";

@Component({
    selector: "app-loading-skeleton",
    templateUrl: "./loading-skeleton.component.html",
    styleUrls: ["./loading-skeleton.component.scss"],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class LoadingSkeletonComponent {
  @Input() skeletonRowsCount = 10; // set to the number of rows you want to display
}
