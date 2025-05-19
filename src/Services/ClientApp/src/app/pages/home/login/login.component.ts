import { Component, OnInit } from '@angular/core';
// import { OAuthService, OAuthErrorEvent, UserInfo } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { FormGroup, FormControl, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoginService } from 'src/app/auth/login.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AdvanceService } from 'src/app/services/advance/advance.service';
import { MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent } from '@angular/material/card';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { NgIf } from '@angular/common';
import { MatButton } from '@angular/material/button';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css'],
    imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, FormsModule, ReactiveFormsModule, MatFormField, MatLabel, MatInput, NgIf, MatError, MatButton]
})
export class LoginComponent implements OnInit {
  public loginForm!: FormGroup;
  constructor(
    private advanceService: AdvanceService,
    private loginService: LoginService,
    private router: Router,
    private snb: MatSnackBar

  ) {
  }
  get username() { return this.loginForm.get('username'); }
  get password() { return this.loginForm.get('password'); }

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

  logout(): void {
    this.loginService.logout();
  }
}
