import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GrnItemSupplierWiseReportComponent } from './grn-item-supplier-wise-report.component';

describe('GrnItemSupplierWiseReportComponent', () => {
  let component: GrnItemSupplierWiseReportComponent;
  let fixture: ComponentFixture<GrnItemSupplierWiseReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GrnItemSupplierWiseReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GrnItemSupplierWiseReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
