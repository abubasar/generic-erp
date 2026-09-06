import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CashBankBalanceReportComponent } from './cash-bank-balance-report.component';

describe('CashBankBalanceReportComponent', () => {
  let component: CashBankBalanceReportComponent;
  let fixture: ComponentFixture<CashBankBalanceReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CashBankBalanceReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CashBankBalanceReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
