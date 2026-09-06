import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseItemWiseSupplierComponent } from './purchase-item-wise-supplier.component';

describe('PurchaseItemWiseSupplierComponent', () => {
  let component: PurchaseItemWiseSupplierComponent;
  let fixture: ComponentFixture<PurchaseItemWiseSupplierComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PurchaseItemWiseSupplierComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseItemWiseSupplierComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
