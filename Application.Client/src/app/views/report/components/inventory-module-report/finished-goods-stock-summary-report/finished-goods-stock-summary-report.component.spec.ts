import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinishedGoodsStockSummaryReportComponent } from './finished-goods-stock-summary-report.component';

describe('FinishedGoodsStockSummaryReportComponent', () => {
  let component: FinishedGoodsStockSummaryReportComponent;
  let fixture: ComponentFixture<FinishedGoodsStockSummaryReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FinishedGoodsStockSummaryReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FinishedGoodsStockSummaryReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
