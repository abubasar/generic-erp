import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentStockPurchaseRateAndQuantityComponent } from './current-stock-purchase-rate-and-quantity.component';

describe('CurrentStockPurchaseRateAndQuantityComponent', () => {
  let component: CurrentStockPurchaseRateAndQuantityComponent;
  let fixture: ComponentFixture<CurrentStockPurchaseRateAndQuantityComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CurrentStockPurchaseRateAndQuantityComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CurrentStockPurchaseRateAndQuantityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
