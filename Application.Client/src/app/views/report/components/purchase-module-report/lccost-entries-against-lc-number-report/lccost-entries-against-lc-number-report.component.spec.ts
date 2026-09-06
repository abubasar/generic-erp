import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LccostEntriesAgainstLcNumberReportComponent } from './lccost-entries-against-lc-number-report.component';

describe('LccostEntriesAgainstLcNumberReportComponent', () => {
  let component: LccostEntriesAgainstLcNumberReportComponent;
  let fixture: ComponentFixture<LccostEntriesAgainstLcNumberReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LccostEntriesAgainstLcNumberReportComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LccostEntriesAgainstLcNumberReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
