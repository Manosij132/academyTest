import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CandidateInterviewsComponent } from './candidate-interviews.component';

describe('CandidateInterviewsComponent', () => {
  let component: CandidateInterviewsComponent;
  let fixture: ComponentFixture<CandidateInterviewsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CandidateInterviewsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CandidateInterviewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
