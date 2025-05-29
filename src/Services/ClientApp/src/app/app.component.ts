import { Component, inject } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { LayoutComponent } from './components/layout/layout.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [LayoutComponent]
})
export class AppComponent {
  private matIconReg = inject(MatIconRegistry);

  title = 'Ater.Dry';
  constructor() {
    this.matIconReg.setDefaultFontSetClass('material-symbols-outlined');
  }
}
