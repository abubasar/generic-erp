import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimaryFinishedGoodsStockReportComponent } from './primary-finished-goods-stock-report.component';

describe('PrimaryFinishedGoodsStockReportComponent', () => {
  let component: PrimaryFinishedGoodsStockReportComponent;
  let fixture: ComponentFixture<PrimaryFinishedGoodsStockReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PrimaryFinishedGoodsStockReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimaryFinishedGoodsStockReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
