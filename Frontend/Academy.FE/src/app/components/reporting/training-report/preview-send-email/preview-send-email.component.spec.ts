import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PreviewSendEmailComponent } from './preview-send-email.component';

describe('PreviewSendEmailComponent', () => {
  let component: PreviewSendEmailComponent;
  let fixture: ComponentFixture<PreviewSendEmailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PreviewSendEmailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PreviewSendEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
