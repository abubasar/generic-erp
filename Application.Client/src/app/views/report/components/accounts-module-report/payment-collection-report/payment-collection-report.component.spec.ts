import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentCollectionReportComponent } from './payment-collection-report.component';

describe('PaymentCollectionReportComponent', () => {
  let component: PaymentCollectionReportComponent;
  let fixture: ComponentFixture<PaymentCollectionReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PaymentCollectionReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PaymentCollectionReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
