import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesSummaryReportComponent } from './sales-summary-report.component';

describe('SalesSummaryReportComponent', () => {
  let component: SalesSummaryReportComponent;
  let fixture: ComponentFixture<SalesSummaryReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesSummaryReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesSummaryReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
