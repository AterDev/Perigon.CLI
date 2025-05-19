import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle } from '@angular/material/expansion';
import { MatSidenavContainer, MatSidenav, MatSidenavContent } from '@angular/material/sidenav';
import { NgIf, NgTemplateOutlet } from '@angular/common';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatNavList, MatListItem } from '@angular/material/list';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
    selector: 'admin-navigation',
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.css'],
    imports: [MatSidenavContainer, MatSidenav, NgIf, MatIconButton, MatIcon, MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle, MatNavList, MatListItem, RouterLink, RouterLinkActive, NgTemplateOutlet, MatSidenavContent, RouterOutlet]
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;
  expanded = true;
  @ViewChild(MatAccordion, { static: true }) accordion!: MatAccordion;
  constructor() { }

  ngOnInit(): void {
    if (this.expanded) {
      this.accordion?.openAll();
    } else {
      this.accordion?.closeAll();
    }
  }

  toggle(): void {
    this.opened = !this.opened;
  }

}
