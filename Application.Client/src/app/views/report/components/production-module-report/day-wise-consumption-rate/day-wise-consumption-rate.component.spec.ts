import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayWiseConsumptionRateComponent } from './day-wise-consumption-rate.component';

describe('DayWiseConsumptionRateComponent', () => {
  let component: DayWiseConsumptionRateComponent;
  let fixture: ComponentFixture<DayWiseConsumptionRateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DayWiseConsumptionRateComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DayWiseConsumptionRateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
