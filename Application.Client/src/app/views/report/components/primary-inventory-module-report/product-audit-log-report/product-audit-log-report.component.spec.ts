import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductAuditLogReportComponent } from './product-audit-log-report.component';

describe('ProductAuditLogReportComponent', () => {
  let component: ProductAuditLogReportComponent;
  let fixture: ComponentFixture<ProductAuditLogReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProductAuditLogReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductAuditLogReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
