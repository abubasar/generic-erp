import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesWarehouseWiseComponent } from './sales-warehouse-wise.component';

describe('SalesWarehouseWiseComponent', () => {
  let component: SalesWarehouseWiseComponent;
  let fixture: ComponentFixture<SalesWarehouseWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesWarehouseWiseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesWarehouseWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
