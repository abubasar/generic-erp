import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerTransactionReportComponent } from './customer-transaction-report.component';

describe('CustomerTransactionReportComponent', () => {
  let component: CustomerTransactionReportComponent;
  let fixture: ComponentFixture<CustomerTransactionReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CustomerTransactionReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerTransactionReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
