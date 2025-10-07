import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemRoleFilterDto } from './models/system-role-filter-dto.model';
import { SystemRoleAddDto } from './models/system-role-add-dto.model';
import { SystemRoleUpdateDto } from './models/system-role-update-dto.model';
import { SystemRoleSetMenusDto } from './models/system-role-set-menus-dto.model';
import { SystemRoleSetPermissionGroupsDto } from './models/system-role-set-permission-groups-dto.model';
import { PageListOfSystemRoleItemDto } from './models/page-list-of-system-role-item-dto.model';
import { SystemRoleDetailDto } from './models/system-role-detail-dto.model';
import { SystemRole2 } from './models/system-role2.model';

/**
 * 
 */
export class SystemRoleBaseService extends BaseService {
  /**
   * filter
   * @param data SystemRoleFilterDto
   */
  filter(data: SystemRoleFilterDto): Observable<PageListOfSystemRoleItemDto> {
    const _url = `/api/SystemRole/filter`;
    return this.request<PageListOfSystemRoleItemDto>('post', _url, data);
  }

  /**
   * add
   * @param data SystemRoleAddDto
   */
  add(data: SystemRoleAddDto): Observable<string> {
    const _url = `/api/SystemRole`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemRoleUpdateDto
   */
  update(id: string, data: SystemRoleUpdateDto): Observable<boolean> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SystemRoleDetailDto> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<SystemRoleDetailDto>('get', _url);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemRole/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * updateMenus
   * @param data SystemRoleSetMenusDto
   */
  updateMenus(data: SystemRoleSetMenusDto): Observable<SystemRole2> {
    const _url = `/api/SystemRole/menus`;
    return this.request<SystemRole2>('put', _url, data);
  }

  /**
   * updatePermissionGroups
   * @param data SystemRoleSetPermissionGroupsDto
   */
  updatePermissionGroups(data: SystemRoleSetPermissionGroupsDto): Observable<SystemRole2> {
    const _url = `/api/SystemRole/permissionGroups`;
    return this.request<SystemRole2>('put', _url, data);
  }

}
