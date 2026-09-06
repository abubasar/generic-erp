import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesModuleReportComponent } from './sales-module-report.component';

describe('SalesModuleReportComponent', () => {
  let component: SalesModuleReportComponent;
  let fixture: ComponentFixture<SalesModuleReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesModuleReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesModuleReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
