import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FullTableViewComponent } from './full-table-view.component';

describe('FullTableViewComponent', () => {
  let component: FullTableViewComponent;
  let fixture: ComponentFixture<FullTableViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FullTableViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FullTableViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
