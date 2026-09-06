import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FinishedGoodsStockReportComponent } from './finished-goods-stock-report.component';

describe('FinishedGoodsStockReportComponent', () => {
  let component: FinishedGoodsStockReportComponent;
  let fixture: ComponentFixture<FinishedGoodsStockReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FinishedGoodsStockReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FinishedGoodsStockReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
