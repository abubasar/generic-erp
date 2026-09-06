import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransitSalesReportComponent } from './transit-sales-report.component';

describe('TransitSalesReportComponent', () => {
  let component: TransitSalesReportComponent;
  let fixture: ComponentFixture<TransitSalesReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TransitSalesReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TransitSalesReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
