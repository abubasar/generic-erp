import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoneyReceiptListComponent } from './money-receipt-list.component';

describe('MoneyReceiptListComponent', () => {
  let component: MoneyReceiptListComponent;
  let fixture: ComponentFixture<MoneyReceiptListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MoneyReceiptListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MoneyReceiptListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
