import { Component } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NavigationStart, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { baseMatModules, commonModules } from 'src/app/share/shared-modules';
import { NavigationComponent } from "./navigation/navigation.component";

@Component({
  selector: 'app-layout',
  imports: [MatToolbarModule, MatMenuModule, ...baseMatModules, ...commonModules, NavigationComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  isLogin = false;
  isAdmin = false;
  username?: string | null = null;
  constructor(
    private auth: AuthService,
    public snb: MatSnackBar,
    private router: Router
  ) {
    router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        console.log(event);
        this.isLogin = this.auth.isLogin;
        this.isAdmin = this.auth.isAdmin;
        this.username = this.auth.userName;
      }
    });
  }

  ngOnInit(): void {
    this.isLogin = this.auth.isLogin;
    this.isAdmin = this.auth.isAdmin;
    this.username = this.auth.userName;
  }

  login(): void {
    this.router.navigateByUrl('/login')
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/index');
    location.reload();
  }

}
