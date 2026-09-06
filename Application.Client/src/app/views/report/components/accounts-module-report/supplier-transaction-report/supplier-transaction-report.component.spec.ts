import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierTransactionReportComponent } from './supplier-transaction-report.component';

describe('SupplierTransactionReportComponent', () => {
  let component: SupplierTransactionReportComponent;
  let fixture: ComponentFixture<SupplierTransactionReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SupplierTransactionReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierTransactionReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
