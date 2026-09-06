import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CashBookReportComponent } from './cash-book-report.component';

describe('CashBookReportComponent', () => {
  let component: CashBookReportComponent;
  let fixture: ComponentFixture<CashBookReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CashBookReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CashBookReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
