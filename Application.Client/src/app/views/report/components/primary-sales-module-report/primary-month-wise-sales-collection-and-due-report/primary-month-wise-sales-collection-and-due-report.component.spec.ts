import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimaryMonthWiseSalesCollectionAndDueReportComponent } from './primary-month-wise-sales-collection-and-due-report.component';

describe('PrimaryMonthWiseSalesCollectionAndDueReportComponent', () => {
  let component: PrimaryMonthWiseSalesCollectionAndDueReportComponent;
  let fixture: ComponentFixture<PrimaryMonthWiseSalesCollectionAndDueReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PrimaryMonthWiseSalesCollectionAndDueReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimaryMonthWiseSalesCollectionAndDueReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
