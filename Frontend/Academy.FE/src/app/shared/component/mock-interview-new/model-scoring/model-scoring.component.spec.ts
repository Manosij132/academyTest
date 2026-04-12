import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModelScoringComponent } from './model-scoring.component';

describe('ModelScoringComponent', () => {
  let component: ModelScoringComponent;
  let fixture: ComponentFixture<ModelScoringComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModelScoringComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModelScoringComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
