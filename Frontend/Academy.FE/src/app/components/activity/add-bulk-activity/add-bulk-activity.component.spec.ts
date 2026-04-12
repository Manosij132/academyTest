import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddBulkActivityComponent } from './add-bulk-activity.component';

describe('AddBulkActivityComponent', () => {
  let component: AddBulkActivityComponent;
  let fixture: ComponentFixture<AddBulkActivityComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddBulkActivityComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddBulkActivityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
