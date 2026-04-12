import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEcosystemDialogComponent } from './add-ecosystem-dialog.component';

describe('AddEcosystemDialogComponent', () => {
  let component: AddEcosystemDialogComponent;
  let fixture: ComponentFixture<AddEcosystemDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEcosystemDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddEcosystemDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
