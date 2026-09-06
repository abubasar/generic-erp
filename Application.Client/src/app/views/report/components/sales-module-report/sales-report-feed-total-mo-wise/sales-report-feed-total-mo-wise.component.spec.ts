import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesReportFeedTotalMoWiseComponent } from './sales-report-feed-total-mo-wise.component';

describe('SalesReportFeedTotalMoWiseComponent', () => {
  let component: SalesReportFeedTotalMoWiseComponent;
  let fixture: ComponentFixture<SalesReportFeedTotalMoWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesReportFeedTotalMoWiseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesReportFeedTotalMoWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
