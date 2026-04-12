/* tslint:disable:no-unused-variable */
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateEndDateDialogComponent } from './update-end-date-dialog.component';

describe('UpdateEndDateDialogComponent', () => {
  let component: UpdateEndDateDialogComponent;
  let fixture: ComponentFixture<UpdateEndDateDialogComponent>;

  beforeEach(async() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateEndDateDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateEndDateDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
