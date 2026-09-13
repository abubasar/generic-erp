import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogRef as MatDialogRef } from '@angular/material/dialog';

@Component({
    selector: 'app-app-loader',
    templateUrl: './app-loader.component.html',
    styleUrls: ['./app-loader.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class AppLoaderComponent implements OnInit {
  title;
  message;
  constructor(public dialogRef: MatDialogRef<AppLoaderComponent>) {}

  ngOnInit() {
  }

}
