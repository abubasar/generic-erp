import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-no-data-found',
    templateUrl: './no-data-found.component.html',
    styleUrls: ['./no-data-found.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class NoDataFoundComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
