import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelEfficiencyEvaluationComponent } from './panel-efficiency-evaluation.component';

describe('PanelEfficiencyGraphDialogComponent', () => {
  let component: PanelEfficiencyEvaluationComponent;
  let fixture: ComponentFixture<PanelEfficiencyEvaluationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PanelEfficiencyEvaluationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PanelEfficiencyEvaluationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});