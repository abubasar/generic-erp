import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductionModuleReportComponent } from './production-module-report.component';

describe('ProductionModuleReportComponent', () => {
  let component: ProductionModuleReportComponent;
  let fixture: ComponentFixture<ProductionModuleReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProductionModuleReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductionModuleReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
