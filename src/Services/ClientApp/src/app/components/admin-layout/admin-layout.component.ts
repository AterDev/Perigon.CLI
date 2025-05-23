import { Component, OnInit } from '@angular/core';
import { NavigationComponent } from '../navigation/navigation.component';

@Component({
    selector: 'app-admin-layout',
    templateUrl: './admin-layout.component.html',
    styleUrls: ['./admin-layout.component.css'],
    imports: [NavigationComponent]
})
export class AdminLayoutComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
