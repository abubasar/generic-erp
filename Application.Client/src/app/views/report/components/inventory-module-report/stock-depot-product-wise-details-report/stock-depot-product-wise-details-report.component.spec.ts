import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockDepotProductWiseDetailsReportComponent } from './stock-depot-product-wise-details-report.component';

describe('StockDepotProductWiseDetailsReportComponent', () => {
  let component: StockDepotProductWiseDetailsReportComponent;
  let fixture: ComponentFixture<StockDepotProductWiseDetailsReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StockDepotProductWiseDetailsReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockDepotProductWiseDetailsReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
