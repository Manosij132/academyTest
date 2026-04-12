import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AimodelsComponent } from './aimodels.component';

describe('AimodelsComponent', () => {
  let component: AimodelsComponent;
  let fixture: ComponentFixture<AimodelsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AimodelsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AimodelsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
