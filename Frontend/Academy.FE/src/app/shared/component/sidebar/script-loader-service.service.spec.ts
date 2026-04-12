import { TestBed } from '@angular/core/testing';

import { ScriptLoaderServiceService } from './script-loader-service.service';

describe('ScriptLoaderServiceService', () => {
  let service: ScriptLoaderServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScriptLoaderServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
