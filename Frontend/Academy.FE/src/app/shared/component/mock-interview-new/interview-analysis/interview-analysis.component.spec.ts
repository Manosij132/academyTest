import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InterviewAnalysisComponent } from './interview-analysis.component';

describe('InterviewAnalysisComponent', () => {
  let component: InterviewAnalysisComponent;
  let fixture: ComponentFixture<InterviewAnalysisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InterviewAnalysisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InterviewAnalysisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
