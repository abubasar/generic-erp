import { ComponentFixture, TestBed } from "@angular/core/testing";

import { SalesItemCustomerWiseComponent } from "./sales-item-customer-wise.component";

describe("SalesItemWiseCustomerComponent", () => {
  let component: SalesItemCustomerWiseComponent;
  let fixture: ComponentFixture<SalesItemCustomerWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SalesItemCustomerWiseComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SalesItemCustomerWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
