import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemMenuFilterDto } from './models/system-menu-filter-dto.model';
import { SystemMenuAddDto } from './models/system-menu-add-dto.model';
import { SystemMenuUpdateDto } from './models/system-menu-update-dto.model';
import { PageListOfSystemMenu } from './models/page-list-of-system-menu.model';

/**
 * 
 */
export class SystemMenuBaseService extends BaseService {
  /**
   * filter
   * @param data SystemMenuFilterDto
   */
  filter(data: SystemMenuFilterDto): Observable<PageListOfSystemMenu> {
    const _url = `/api/SystemMenu/filter`;
    return this.request<PageListOfSystemMenu>('post', _url, data);
  }

  /**
   * add
   * @param data SystemMenuAddDto
   */
  add(data: SystemMenuAddDto): Observable<string> {
    const _url = `/api/SystemMenu`;
    return this.request<string>('post', _url, data);
  }

  /**
   * update
   * @param id string
   * @param data SystemMenuUpdateDto
   */
  update(id: string, data: SystemMenuUpdateDto): Observable<boolean> {
    const _url = `/api/SystemMenu/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * delete
   * @param id string
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/SystemMenu/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
