import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemPermissionFilterDto } from './models/system-permission-filter-dto.model';
import { SystemPermissionAddDto } from './models/system-permission-add-dto.model';
import { SystemPermissionUpdateDto } from './models/system-permission-update-dto.model';
import { PageListOfSystemPermissionItemDto } from './models/page-list-of-system-permission-item-dto.model';
import { SystemPermissionDetailDto } from './models/system-permission-detail-dto.model';

/**
 * 
 */
export class SystemPermissionBaseService extends BaseService {
  /**
   * filter
   * @param data SystemPermissionFilterDto
   */
  filter(data: SystemPermissionFilterDto): Observable<PageListOfSystemPermissionItemDto> {
    const _url = `/api/SystemPermission/filter`;
    return this.request<PageListOfSystemPermissionItemDto>('post', _url, data);
  }

  /**
   * add
   * @param data SystemPermissionAddDto
   */
  add(data: SystemPermissionAddDto): Observable<string> {
    const _url = `/api/SystemPermission`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemPermissionUpdateDto
   */
  update(id: string, data: SystemPermissionUpdateDto): Observable<boolean> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SystemPermissionDetailDto> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<SystemPermissionDetailDto>('get', _url);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemPermission/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
