import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayWiseConsumptionQuantityComponent } from './day-wise-consumption-quantity.component';

describe('DayWiseConsumptionQuantityComponent', () => {
  let component: DayWiseConsumptionQuantityComponent;
  let fixture: ComponentFixture<DayWiseConsumptionQuantityComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DayWiseConsumptionQuantityComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DayWiseConsumptionQuantityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
