import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatMultiSelectComponent } from './mat-multi-select.component';

describe('MatMultiSelectComponent', () => {
  let component: MatMultiSelectComponent;
  let fixture: ComponentFixture<MatMultiSelectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatMultiSelectComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatMultiSelectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
