import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesReportFeedTotalCustomerWiseComponent } from './sales-report-feed-total-customer-wise.component';

describe('SalesReportFeedTotalCustomerWiseComponent', () => {
  let component: SalesReportFeedTotalCustomerWiseComponent;
  let fixture: ComponentFixture<SalesReportFeedTotalCustomerWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesReportFeedTotalCustomerWiseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesReportFeedTotalCustomerWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
