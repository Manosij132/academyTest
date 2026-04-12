import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { interviewAuthGuard } from './interview-auth.guard';

describe('interviewAuthGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => interviewAuthGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
