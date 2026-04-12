import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateAbilityKnowledgeDialogComponent } from './update-ability-knowledge-dialog.component';

describe('UpdateAbilityKnowledgeDialogComponent', () => {
  let component: UpdateAbilityKnowledgeDialogComponent;
  let fixture: ComponentFixture<UpdateAbilityKnowledgeDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateAbilityKnowledgeDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UpdateAbilityKnowledgeDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
