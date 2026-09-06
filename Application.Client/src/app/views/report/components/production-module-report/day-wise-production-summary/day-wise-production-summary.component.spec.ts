import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayWiseProductionSummaryComponent } from './day-wise-production-summary.component';

describe('DayWiseProductionSummaryComponent', () => {
  let component: DayWiseProductionSummaryComponent;
  let fixture: ComponentFixture<DayWiseProductionSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DayWiseProductionSummaryComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DayWiseProductionSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
