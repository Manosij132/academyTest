import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MockInterviewDetailsComponent } from './mock-interview-details.component';

describe('MockInterviewDetailsComponent', () => {
  let component: MockInterviewDetailsComponent;
  let fixture: ComponentFixture<MockInterviewDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MockInterviewDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MockInterviewDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
