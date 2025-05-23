import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StringComponent } from './string.component';

describe('StringComponent', () => {
  let component: StringComponent;
  let fixture: ComponentFixture<StringComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StringComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(StringComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
