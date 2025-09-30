
import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
// import { OAuthService, OAuthErrorEvent, UserInfo } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { SystemUserService } from 'src/app/services/admin/system-user.service';
import { AuthService } from 'src/app/services/auth.service';
import { AdminClient } from 'src/app/services/admin/admin-client';

@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login implements OnInit {
  public loginForm!: FormGroup;
  private adminClient = inject(AdminClient);
  constructor(
    private authService: AuthService,
    private service: SystemUserService,
    private router: Router
  ) {
    if (authService.isLogin) {
      if (this.service.isMobile) {
        this.router.navigate(['/mobile']);
      } else {
        this.router.navigate(['/system-role']);
      }
    }
  }

  get username() {
    return this.loginForm.get('username');
  }
  get password() {
    return this.loginForm.get('password');
  }

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.minLength(3)]),
      password: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(50)])
    });
  }

  /**
   * 错误信息
   * @param type 字段名称
   */
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'username':
        return this.username?.errors?.['required'] ? '用户名必填' :
          this.username?.errors?.['minlength']
            || this.username?.errors?.['maxlength'] ? '用户名长度3-20位' : '';
      case 'password':
        return this.password?.errors?.['required'] ? '密码必填' :
          this.password?.errors?.['minlength'] ? '密码长度不可低于6位' :
            this.password?.errors?.['maxlength'] ? '密码长度不可超过50' : '';
      default:
        break;
    }
    return '';
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
