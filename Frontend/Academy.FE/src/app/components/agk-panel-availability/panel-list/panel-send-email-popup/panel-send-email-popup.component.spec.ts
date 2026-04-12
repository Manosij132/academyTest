import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelSendEmailPopupComponent } from './panel-send-email-popup.component';

describe('PanelSendEmailPopupComponent', () => {
  let component: PanelSendEmailPopupComponent;
  let fixture: ComponentFixture<PanelSendEmailPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PanelSendEmailPopupComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PanelSendEmailPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
