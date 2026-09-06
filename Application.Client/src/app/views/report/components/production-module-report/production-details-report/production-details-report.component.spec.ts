import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductionDetailsReportComponent } from './production-details-report.component';

describe('ProductionDetailsReportComponent', () => {
  let component: ProductionDetailsReportComponent;
  let fixture: ComponentFixture<ProductionDetailsReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProductionDetailsReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductionDetailsReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
