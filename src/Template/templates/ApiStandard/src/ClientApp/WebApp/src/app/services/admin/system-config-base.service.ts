import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemConfigFilterDto } from './models/system-config-filter-dto.model';
import { SystemConfigAddDto } from './models/system-config-add-dto.model';
import { SystemConfigUpdateDto } from './models/system-config-update-dto.model';
import { PageListOfSystemConfigItemDto } from './models/page-list-of-system-config-item-dto.model';
import { EnumDictionary } from './models/enum-dictionary.model';
import { SystemConfigDetailDto } from './models/system-config-detail-dto.model';

/**
 * 
 */
export class SystemConfigBaseService extends BaseService {
  /**
   * filter
   * @param data SystemConfigFilterDto
   */
  filter(data: SystemConfigFilterDto): Observable<PageListOfSystemConfigItemDto> {
    const _url = `/api/SystemConfig/filter`;
    return this.request<PageListOfSystemConfigItemDto>('post', _url, data);
  }

  /**
   * getEnumConfigs
   */
  getEnumConfigs(): Observable<Map<string, EnumDictionary[]>> {
    const _url = `/api/SystemConfig/enum`;
    return this.request<Map<string, EnumDictionary[]>>('get', _url);
  }

  /**
   * add
   * @param data SystemConfigAddDto
   */
  add(data: SystemConfigAddDto): Observable<string> {
    const _url = `/api/SystemConfig`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemConfigUpdateDto
   */
  update(id: string, data: SystemConfigUpdateDto): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * getDetail
   * @param id string
   */
  getDetail(id: string): Observable<SystemConfigDetailDto> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<SystemConfigDetailDto>('get', _url);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemConfig/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
