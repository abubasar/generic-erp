import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RawMaterialsStockReportComponent } from './raw-materials-stock-report.component';

describe('RawMaterialsStockReportComponent', () => {
  let component: RawMaterialsStockReportComponent;
  let fixture: ComponentFixture<RawMaterialsStockReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RawMaterialsStockReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RawMaterialsStockReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
