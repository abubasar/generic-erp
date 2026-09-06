import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseModuleReportComponent } from './purchase-module-report.component';

describe('PurchaseModuleReportComponent', () => {
  let component: PurchaseModuleReportComponent;
  let fixture: ComponentFixture<PurchaseModuleReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PurchaseModuleReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseModuleReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
