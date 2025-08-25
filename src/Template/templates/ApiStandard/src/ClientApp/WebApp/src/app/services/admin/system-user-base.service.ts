import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemLoginDto } from './models/system-login-dto.model';
import { SystemUserFilterDto } from './models/system-user-filter-dto.model';
import { SystemUserAddDto } from './models/system-user-add-dto.model';
import { SystemUserUpdateDto } from './models/system-user-update-dto.model';
import { AuthResult } from './models/auth-result.model';
import { AccessTokenDto } from './models/access-token-dto.model';
import { PageListOfSystemUserItemDto } from './models/page-list-of-system-user-item-dto.model';
import { SystemUserDetailDto } from './models/system-user-detail-dto.model';

/**
 * 
 */
export class SystemUserBaseService extends BaseService {
  /**
   * sendVerifyCode
   * @param email string
   */
  sendVerifyCode(email: string | null): Observable<any> {
    const _url = `/api/SystemUser/verifyCode?email=${email ?? ''}`;
    return this.request<any>('post', _url);
  }

  /**
   * getCaptchaImage
   */
  getCaptchaImage(): Observable<any> {
    const _url = `/api/SystemUser/captcha`;
    return this.request<any>('get', _url);
  }

  /**
   * login
   * @param data SystemLoginDto
   */
  login(data: SystemLoginDto): Observable<AuthResult> {
    const _url = `/api/SystemUser/login`;
    return this.request<AuthResult>('post', _url, data);
  }

  /**
   * refreshToken
   * @param refreshToken string
   */
  refreshToken(refreshToken: string | null): Observable<AccessTokenDto> {
    const _url = `/api/SystemUser/refresh_token?refreshToken=${refreshToken ?? ''}`;
    return this.request<AccessTokenDto>('get', _url);
  }

  /**
   * logout
   * @param id string
   */
  logout(id: string): Observable<boolean> {
    const _url = `/api/SystemUser/logout/${id}`;
    return this.request<boolean>('post', _url);
  }

  /**
   * filter
   * @param data SystemUserFilterDto
   */
  filter(data: SystemUserFilterDto): Observable<PageListOfSystemUserItemDto> {
    const _url = `/api/SystemUser/filter`;
    return this.request<PageListOfSystemUserItemDto>('post', _url, data);
  }

  /**
   * add
   * @param data SystemUserAddDto
   */
  add(data: SystemUserAddDto): Observable<string> {
    const _url = `/api/SystemUser`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemUserUpdateDto
   */
  update(id: string, data: SystemUserUpdateDto): Observable<boolean> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SystemUserDetailDto> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<SystemUserDetailDto>('get', _url);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemUser/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * changePassword
   * @param password string
   * @param newPassword string
   */
  changePassword(password: string | null, newPassword: string | null): Observable<boolean> {
    const _url = `/api/SystemUser/changePassword?password=${password ?? ''}&newPassword=${newPassword ?? ''}`;
    return this.request<boolean>('patch', _url);
  }

}
