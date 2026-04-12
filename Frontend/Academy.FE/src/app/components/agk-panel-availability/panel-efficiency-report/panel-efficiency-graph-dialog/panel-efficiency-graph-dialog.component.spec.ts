import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelEfficiencyGraphDialogComponent } from './panel-efficiency-graph-dialog.component';

describe('PanelEfficiencyGraphDialogComponent', () => {
  let component: PanelEfficiencyGraphDialogComponent;
  let fixture: ComponentFixture<PanelEfficiencyGraphDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PanelEfficiencyGraphDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PanelEfficiencyGraphDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});