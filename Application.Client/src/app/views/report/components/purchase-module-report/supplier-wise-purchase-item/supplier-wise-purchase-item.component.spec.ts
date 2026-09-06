import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierWisePurchaseItemComponent } from './supplier-wise-purchase-item.component';

describe('SupplierWisePurchaseItemComponent', () => {
  let component: SupplierWisePurchaseItemComponent;
  let fixture: ComponentFixture<SupplierWisePurchaseItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SupplierWisePurchaseItemComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierWisePurchaseItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
