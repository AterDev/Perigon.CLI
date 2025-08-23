import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { BaseService } from 'src/app/services/admin/base.service';

@Component({
  selector: 'app-notfound',
  templateUrl: './notfound.html',
  styleUrl: './notfound.css'
})
export class Notfound {
  constructor(
    private baseService: BaseService,
    private router: Router,
  ) {
    if (this.baseService.isMobile) {

      this.router.navigateByUrl('/mobile');
    } else {
      this.router.navigateByUrl('/admin');
    }
  }
}
