/* tslint:disable:no-unused-variable */
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateTrainingImpactDialogComponent } from './update-training-impact-dialog.component';

describe('UpdateTrainingImpactDialogComponent', () => {
  let component: UpdateTrainingImpactDialogComponent;
  let fixture: ComponentFixture<UpdateTrainingImpactDialogComponent>;

  beforeEach(async() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateTrainingImpactDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateTrainingImpactDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
