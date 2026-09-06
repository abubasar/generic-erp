import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SaleTotalProductWiseReportComponent } from './sale-total-product-wise-report.component';

describe('SaleTotalProductWiseReportComponent', () => {
  let component: SaleTotalProductWiseReportComponent;
  let fixture: ComponentFixture<SaleTotalProductWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SaleTotalProductWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SaleTotalProductWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
