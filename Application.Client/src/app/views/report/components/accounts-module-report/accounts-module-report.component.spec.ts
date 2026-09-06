import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountsModuleReportComponent } from './accounts-module-report.component';

describe('AccountsModuleReportComponent', () => {
  let component: AccountsModuleReportComponent;
  let fixture: ComponentFixture<AccountsModuleReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AccountsModuleReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountsModuleReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
