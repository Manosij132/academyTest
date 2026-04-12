import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkActivityDialogComponent } from './bulk-activity-dialog.component';

describe('BulkActivityDialogComponent', () => {
  let component: BulkActivityDialogComponent;
  let fixture: ComponentFixture<BulkActivityDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkActivityDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BulkActivityDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
