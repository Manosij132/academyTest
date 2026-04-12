import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MockInterviewLayoutComponentComponent } from './mock-interview-layout-component.component';

describe('MockInterviewLayoutComponentComponent', () => {
  let component: MockInterviewLayoutComponentComponent;
  let fixture: ComponentFixture<MockInterviewLayoutComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MockInterviewLayoutComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MockInterviewLayoutComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
