import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateEcosystemComponent } from './create-ecosystem.component';

describe('CreateEcosystemComponent', () => {
  let component: CreateEcosystemComponent;
  let fixture: ComponentFixture<CreateEcosystemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateEcosystemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateEcosystemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
