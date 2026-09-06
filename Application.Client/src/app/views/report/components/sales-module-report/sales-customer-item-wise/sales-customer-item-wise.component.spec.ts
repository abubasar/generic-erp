import { ComponentFixture, TestBed } from "@angular/core/testing";

import { SalesCustomerItemWiseComponent } from "./sales-customer-item-wise.component";

describe("CustomerWiseSalesItemComponent", () => {
  let component: SalesCustomerItemWiseComponent;
  let fixture: ComponentFixture<SalesCustomerItemWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SalesCustomerItemWiseComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SalesCustomerItemWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
