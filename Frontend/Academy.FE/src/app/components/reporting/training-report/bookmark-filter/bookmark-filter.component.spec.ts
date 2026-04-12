import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BookmarkFilterComponent } from './bookmark-filter.component';

describe('BookmarkFilterComponent', () => {
  let component: BookmarkFilterComponent;
  let fixture: ComponentFixture<BookmarkFilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookmarkFilterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BookmarkFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
