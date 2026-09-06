import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyTransactionDetailReportComponent } from './daily-transaction-detail-report.component';

describe('DailyTransactionDetailReportComponent', () => {
  let component: DailyTransactionDetailReportComponent;
  let fixture: ComponentFixture<DailyTransactionDetailReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DailyTransactionDetailReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DailyTransactionDetailReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
