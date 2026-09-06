import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoneyReceiptDetailComponent } from './money-receipt-detail.component';

describe('MoneyReceiptDetailComponent', () => {
  let component: MoneyReceiptDetailComponent;
  let fixture: ComponentFixture<MoneyReceiptDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MoneyReceiptDetailComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MoneyReceiptDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
