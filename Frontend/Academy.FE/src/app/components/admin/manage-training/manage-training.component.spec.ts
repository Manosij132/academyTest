import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageTrainingComponent } from './manage-training.component';

describe('ManageTrainingComponent', () => {
  let component: ManageTrainingComponent;
  let fixture: ComponentFixture<ManageTrainingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageTrainingComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageTrainingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
