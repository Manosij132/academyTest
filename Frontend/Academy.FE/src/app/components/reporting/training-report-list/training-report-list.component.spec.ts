import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainingReportListComponent } from './training-report-list.component';

describe('TrainingReportListComponent', () => {
  let component: TrainingReportListComponent;
  let fixture: ComponentFixture<TrainingReportListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainingReportListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrainingReportListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
