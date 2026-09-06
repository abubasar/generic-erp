import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayWiseAccountLedgerComponent } from './day-wise-account-ledger.component';

describe('DayWiseAccountLedgerComponent', () => {
  let component: DayWiseAccountLedgerComponent;
  let fixture: ComponentFixture<DayWiseAccountLedgerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DayWiseAccountLedgerComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DayWiseAccountLedgerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
