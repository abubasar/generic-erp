import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent } from './primary-marketing-officer-wise-sales-collection-and-due-report.component';

describe('PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent', () => {
  let component: PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent;
  let fixture: ComponentFixture<PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
