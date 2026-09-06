import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesOrderHistoryReportComponent } from './sales-order-history-report.component';

describe('SalesOrderHistoryReportComponent', () => {
  let component: SalesOrderHistoryReportComponent;
  let fixture: ComponentFixture<SalesOrderHistoryReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesOrderHistoryReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesOrderHistoryReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
