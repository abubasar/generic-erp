import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerLedgerFeedWiseReportComponent } from './customer-ledger-feed-wise-report.component';

describe('CustomerLedgerFeedWiseReportComponent', () => {
  let component: CustomerLedgerFeedWiseReportComponent;
  let fixture: ComponentFixture<CustomerLedgerFeedWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CustomerLedgerFeedWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerLedgerFeedWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
