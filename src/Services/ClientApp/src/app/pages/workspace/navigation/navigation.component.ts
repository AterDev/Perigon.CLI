import { Component, OnInit, ViewChild } from '@angular/core';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle } from '@angular/material/expansion';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatDrawerContainer, MatDrawer, MatDrawerContent } from '@angular/material/sidenav';
import { NgIf } from '@angular/common';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { MatNavList, MatListItem } from '@angular/material/list';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss'],
  imports: [MatDrawerContainer, MatDrawer, NgIf, MatIconButton, MatIcon, MatAccordion, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle, MatTooltip, MatNavList, MatListItem, RouterLink, RouterLinkActive, MatDrawerContent, RouterOutlet]
})
export class NavigationComponent implements OnInit {
  events: string[] = [];
  opened = true;
  expanded = true;
  isNodeJs = false;
  @ViewChild(MatAccordion, { static: true }) accordion!: MatAccordion;
  constructor(
    public route: ActivatedRoute,
  ) {
    const project = localStorage.getItem('project');
    if (project) {
      let solutionType = JSON.parse(project).solutionType;
      if (solutionType === 1) {
        this.isNodeJs = true;
      }
    }
  }

  ngOnInit(): void {
  }

  toggle(): void {
    this.opened = !this.opened;
  }

  expandToggle(): void {
    this.expanded = !this.expanded;
    if (this.expanded) {
      this.accordion?.openAll();
    } else {
      this.accordion?.closeAll();
    }
  }
}
