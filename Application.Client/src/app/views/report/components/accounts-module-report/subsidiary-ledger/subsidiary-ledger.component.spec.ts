import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubsidiaryLedgerComponent } from './subsidiary-ledger.component';

describe('SubsidiaryLedgerComponent', () => {
  let component: SubsidiaryLedgerComponent;
  let fixture: ComponentFixture<SubsidiaryLedgerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SubsidiaryLedgerComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubsidiaryLedgerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
