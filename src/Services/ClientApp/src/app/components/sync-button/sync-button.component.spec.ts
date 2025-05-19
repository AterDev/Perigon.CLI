import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SyncButtonComponent } from './sync-button.component';

describe('SyncButtonComponent', () => {
  let component: SyncButtonComponent;
  let fixture: ComponentFixture<SyncButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SyncButtonComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SyncButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
