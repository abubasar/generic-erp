import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockDepotProductWiseShortReportComponent } from './stock-depot-product-wise-short-report.component';

describe('StockDepotProductWiseShortReportComponent', () => {
  let component: StockDepotProductWiseShortReportComponent;
  let fixture: ComponentFixture<StockDepotProductWiseShortReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StockDepotProductWiseShortReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockDepotProductWiseShortReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
