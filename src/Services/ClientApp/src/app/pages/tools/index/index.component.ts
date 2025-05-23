import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule, MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent } from '@angular/material/card';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-index',
    templateUrl: './index.component.html',
    styleUrl: './index.component.css',
    imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent]
})
export class IndexComponent {
  constructor(
    private router: Router,

  ) {

  }
  goTo(toolName: string): void {
    this.router.navigateByUrl('/tools/' + toolName)
  }

  openInNewTab(url: string): void {
    window.open(url, '_blank');
  }
}
