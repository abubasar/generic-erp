import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TotalPurchaseItemComponent } from './total-purchase-item.component';

describe('TotalPurchaseItemComponent', () => {
  let component: TotalPurchaseItemComponent;
  let fixture: ComponentFixture<TotalPurchaseItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TotalPurchaseItemComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TotalPurchaseItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
