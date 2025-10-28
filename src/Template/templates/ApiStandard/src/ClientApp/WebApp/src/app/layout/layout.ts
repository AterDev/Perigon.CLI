import { Component, inject } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NavigationStart, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NavigationComponent } from "./navigation/navigation";
import { TranslateService } from '@ngx-translate/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { I18N_KEYS } from '../share/i18n-keys';
import { BaseMatModules, CommonModules } from '../share/shared-modules';

@Component({
  selector: 'app-layout',
  imports: [MatToolbarModule, MatMenuModule, ...BaseMatModules, ...CommonModules,
    NavigationComponent, MatButtonToggleModule],
  templateUrl: './layout.html',
  styleUrl: './layout.scss'
})
export class LayoutComponent {
  isLogin = false;
  isAdmin = false;
  username?: string | null = null;
  i18n = I18N_KEYS;
  translate = inject(TranslateService);
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

  setLanguage(lang: string) {
    this.translate.use(lang);
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
