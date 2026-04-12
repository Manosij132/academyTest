import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelSlotCalendarPopupComponent } from './panel-slot-calendar-popup.component';

describe('PanelSlotCalendarPopupComponent', () => {
  let component: PanelSlotCalendarPopupComponent;
  let fixture: ComponentFixture<PanelSlotCalendarPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PanelSlotCalendarPopupComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PanelSlotCalendarPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
