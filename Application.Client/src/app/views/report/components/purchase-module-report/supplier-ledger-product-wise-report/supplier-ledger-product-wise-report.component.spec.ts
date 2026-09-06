import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierLedgerProductWiseReportComponent } from './supplier-ledger-product-wise-report.component';

describe('SupplierLedgerProductWiseReportComponent', () => {
  let component: SupplierLedgerProductWiseReportComponent;
  let fixture: ComponentFixture<SupplierLedgerProductWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SupplierLedgerProductWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierLedgerProductWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
