import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScheduleMockInterviewDialogComponent } from './schedule-mock-interview-dialog.component';

describe('ScheduleMockInterviewDialogComponent', () => {
  let component: ScheduleMockInterviewDialogComponent;
  let fixture: ComponentFixture<ScheduleMockInterviewDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScheduleMockInterviewDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ScheduleMockInterviewDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
