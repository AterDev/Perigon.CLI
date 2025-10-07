import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemPermissionGroupFilterDto } from './models/system-permission-group-filter-dto.model';
import { SystemPermissionGroupAddDto } from './models/system-permission-group-add-dto.model';
import { SystemPermissionGroupUpdateDto } from './models/system-permission-group-update-dto.model';
import { PageListOfSystemPermissionGroupItemDto } from './models/page-list-of-system-permission-group-item-dto.model';
import { SystemPermissionGroupDetailDto } from './models/system-permission-group-detail-dto.model';

/**
 * 
 */
export class SystemPermissionGroupBaseService extends BaseService {
  /**
   * filter
   * @param data SystemPermissionGroupFilterDto
   */
  filter(data: SystemPermissionGroupFilterDto): Observable<PageListOfSystemPermissionGroupItemDto> {
    const _url = `/api/SystemPermissionGroup/filter`;
    return this.request<PageListOfSystemPermissionGroupItemDto>('post', _url, data);
  }

  /**
   * add
   * @param data SystemPermissionGroupAddDto
   */
  add(data: SystemPermissionGroupAddDto): Observable<string> {
    const _url = `/api/SystemPermissionGroup`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemPermissionGroupUpdateDto
   */
  update(id: string, data: SystemPermissionGroupUpdateDto): Observable<boolean> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SystemPermissionGroupDetailDto> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<SystemPermissionGroupDetailDto>('get', _url);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemPermissionGroup/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
