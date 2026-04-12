import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelEfficiencyReportComponent } from './panel-efficiency-report.component';

describe('PanelEfficiencyReportComponent', () => {
  let component: PanelEfficiencyReportComponent;
  let fixture: ComponentFixture<PanelEfficiencyReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PanelEfficiencyReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PanelEfficiencyReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
