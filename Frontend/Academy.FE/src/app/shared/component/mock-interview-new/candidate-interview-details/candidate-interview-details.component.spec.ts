import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CandidateInterviewDetailsComponent } from './candidate-interview-details.component';

describe('CandidateInterviewDetailsComponent', () => {
  let component: CandidateInterviewDetailsComponent;
  let fixture: ComponentFixture<CandidateInterviewDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CandidateInterviewDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CandidateInterviewDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
