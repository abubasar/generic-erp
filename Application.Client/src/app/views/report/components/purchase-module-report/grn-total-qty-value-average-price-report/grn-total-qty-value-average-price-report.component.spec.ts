import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GrnTotalQtyValueAveragePriceReportComponent } from './grn-total-qty-value-average-price-report.component';

describe('GrnTotalQtyValueAveragePriceReportComponent', () => {
  let component: GrnTotalQtyValueAveragePriceReportComponent;
  let fixture: ComponentFixture<GrnTotalQtyValueAveragePriceReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GrnTotalQtyValueAveragePriceReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GrnTotalQtyValueAveragePriceReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
