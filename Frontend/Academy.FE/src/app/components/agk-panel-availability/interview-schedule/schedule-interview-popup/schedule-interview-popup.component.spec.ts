import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScheduleInterviewPopupComponent } from './schedule-interview-popup.component';


describe('ScheduleInterviewPopupComponent', () => {
  let component: ScheduleInterviewPopupComponent;
  let fixture: ComponentFixture<ScheduleInterviewPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ScheduleInterviewPopupComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ScheduleInterviewPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
