import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesMonthWiseComponent } from './sales-month-wise.component';

describe('SalesMonthWiseComponent', () => {
  let component: SalesMonthWiseComponent;
  let fixture: ComponentFixture<SalesMonthWiseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesMonthWiseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesMonthWiseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
