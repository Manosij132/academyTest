import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PreviewEmailModalComponent } from './preview-email-modal.component';

describe('PreviewEmailModalComponent', () => {
  let component: PreviewEmailModalComponent;
  let fixture: ComponentFixture<PreviewEmailModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PreviewEmailModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PreviewEmailModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
