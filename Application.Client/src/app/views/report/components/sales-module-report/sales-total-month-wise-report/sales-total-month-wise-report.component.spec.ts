import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesTotalMonthWiseReportComponent } from './sales-total-month-wise-report.component';

describe('SalesTotalMonthWiseReportComponent', () => {
  let component: SalesTotalMonthWiseReportComponent;
  let fixture: ComponentFixture<SalesTotalMonthWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesTotalMonthWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesTotalMonthWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
