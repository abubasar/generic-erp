import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyTransactionReportComponent } from './daily-transaction-report.component';

describe('DailyTransactionReportComponent', () => {
  let component: DailyTransactionReportComponent;
  let fixture: ComponentFixture<DailyTransactionReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DailyTransactionReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DailyTransactionReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
