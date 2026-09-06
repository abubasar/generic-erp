import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GrnSupplierItemWiseReportComponent } from './grn-supplier-item-wise-report.component';

describe('GrnSupplierItemWiseReportComponent', () => {
  let component: GrnSupplierItemWiseReportComponent;
  let fixture: ComponentFixture<GrnSupplierItemWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GrnSupplierItemWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GrnSupplierItemWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
