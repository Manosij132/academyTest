import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteinterviewComponent } from './deleteinterview.component';

describe('DeleteinterviewComponent', () => {
  let component: DeleteinterviewComponent;
  let fixture: ComponentFixture<DeleteinterviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteinterviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteinterviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
