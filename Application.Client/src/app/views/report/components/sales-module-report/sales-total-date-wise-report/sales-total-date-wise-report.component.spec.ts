import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesTotalDateWiseReportComponent } from './sales-total-date-wise-report.component';

describe('SalesTotalDateWiseReportComponent', () => {
  let component: SalesTotalDateWiseReportComponent;
  let fixture: ComponentFixture<SalesTotalDateWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesTotalDateWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesTotalDateWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
