import { Component, inject, OnInit, AfterViewInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
// import { OAuthService, OAuthErrorEvent, UserInfo } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { SystemUserService } from 'src/app/services/admin/system-user.service';
import { AuthService } from 'src/app/services/auth.service';
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { initStarfield } from './starfield';

@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login implements OnInit, AfterViewInit {
  ngAfterViewInit(): void {
    const canvas = document.getElementById('starfield') as HTMLCanvasElement | null;
    if (canvas) {
      initStarfield(canvas);
    }
  }
  public loginForm!: FormGroup;
  i18nKeys = I18N_KEYS;
  private adminClient = inject(AdminClient);
  private translate = inject(TranslateService);
  constructor(
    private authService: AuthService,
    private service: SystemUserService,
    private router: Router
  ) {
    if (authService.isLogin) {
      this.router.navigate(['/system-role']);
    }
  }

  get username() {
    return this.loginForm.get('username') as FormControl;
  }
  get password() {
    return this.loginForm.get('password') as FormControl;
  }

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(50)]),
      password: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(60)])
    });
  }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) {
      return '';
    }
    const errors: ValidationErrors = control.errors;
    const errorKeys = Object.keys(errors);
    if (errorKeys.length === 0) {
      return '';
    }

    const key = errorKeys[0];
    const params = errors[key];
    const translationKey = `validation.${key.toLowerCase()}`;
    return this.translate.instant(translationKey, params);
  }

  doLogin(): void {
    const data = this.loginForm.value;
    // 登录接口
    this.adminClient.systemUser.login(data)
      .subscribe(res => {
        this.authService.saveLoginState(res.username, res.accessToken);
        this.router.navigate(['/system-role']);
      });
  }


  logout(): void {
    this.authService.logout();
  }
}
