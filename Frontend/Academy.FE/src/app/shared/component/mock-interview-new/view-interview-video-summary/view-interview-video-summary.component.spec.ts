import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewInterviewVideoSummaryComponent } from './view-interview-video-summary.component';

describe('ViewInterviewVideoSummaryComponent', () => {
  let component: ViewInterviewVideoSummaryComponent;
  let fixture: ComponentFixture<ViewInterviewVideoSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewInterviewVideoSummaryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewInterviewVideoSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
