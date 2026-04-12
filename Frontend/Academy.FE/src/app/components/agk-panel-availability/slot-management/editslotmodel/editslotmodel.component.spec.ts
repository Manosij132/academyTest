import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditSlotmodelComponent } from './editslotmodel.component';

describe('EditslotmodelComponent', () => {
  let component: EditSlotmodelComponent;
  let fixture: ComponentFixture<EditSlotmodelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditSlotmodelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EditSlotmodelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
