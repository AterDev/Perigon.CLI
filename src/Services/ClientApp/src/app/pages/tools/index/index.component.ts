import { Component, inject } from '@angular/core';

import { MatCardModule, MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent } from '@angular/material/card';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-index',
    templateUrl: './index.component.html',
    styleUrl: './index.component.css',
    imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent]
})
export class IndexComponent {
  private router = inject(Router);

  goTo(toolName: string): void {
    this.router.navigateByUrl('/tools/' + toolName)
  }

  openInNewTab(url: string): void {
    window.open(url, '_blank');
  }
}
