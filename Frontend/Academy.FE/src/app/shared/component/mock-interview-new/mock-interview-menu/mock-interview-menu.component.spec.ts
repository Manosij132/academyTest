import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MockInterviewMenuComponent } from './mock-interview-menu.component';

describe('MockInterviewMenuComponent', () => {
  let component: MockInterviewMenuComponent;
  let fixture: ComponentFixture<MockInterviewMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MockInterviewMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MockInterviewMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
