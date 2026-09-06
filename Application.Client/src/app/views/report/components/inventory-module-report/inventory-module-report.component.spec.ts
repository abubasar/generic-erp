import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryModuleReportComponent } from './inventory-module-report.component';

describe('InventoryModuleReportComponent', () => {
  let component: InventoryModuleReportComponent;
  let fixture: ComponentFixture<InventoryModuleReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InventoryModuleReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryModuleReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
