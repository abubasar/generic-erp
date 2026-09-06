import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerLedgerProductWiseReportComponent } from './customer-ledger-product-wise-report.component';

describe('CustomerLedgerProductWiseReportComponent', () => {
  let component: CustomerLedgerProductWiseReportComponent;
  let fixture: ComponentFixture<CustomerLedgerProductWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CustomerLedgerProductWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerLedgerProductWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
