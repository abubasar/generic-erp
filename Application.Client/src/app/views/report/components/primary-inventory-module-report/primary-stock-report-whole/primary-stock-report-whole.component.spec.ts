import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimaryStockReportWholeComponent } from './primary-stock-report-whole.component';

describe('PrimaryStockReportWholeComponent', () => {
  let component: PrimaryStockReportWholeComponent;
  let fixture: ComponentFixture<PrimaryStockReportWholeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PrimaryStockReportWholeComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimaryStockReportWholeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
