import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { GenStepFilterDto } from './models/gen-step-filter-dto.model';
import { GenStepAddDto } from './models/gen-step-add-dto.model';
import { GenStepUpdateDto } from './models/gen-step-update-dto.model';
import { GenStepItemDtoPageList } from './models/gen-step-item-dto-page-list.model';
import { GenStepDetailDto } from './models/gen-step-detail-dto.model';

/**
 * task step
 */
@Injectable({ providedIn: 'root' })
export class GenStepBaseService extends BaseService {
  /**
   * 分页数据
   * @param data GenStepFilterDto
   */
  filter(data: GenStepFilterDto): Observable<GenStepItemDtoPageList> {
    const _url = `/api/admin/GenStep/filter`;
    return this.request<GenStepItemDtoPageList>('post', _url, data);
  }

  /**
   * 新增
   * @param data GenStepAddDto
   */
  add(data: GenStepAddDto): Observable<string> {
    const _url = `/api/admin/GenStep`;
    return this.request<string>('post', _url, data);
  }

  /**
   * 更新数据
   * @param id 
   * @param data GenStepUpdateDto
   */
  update(id: string, data: GenStepUpdateDto): Observable<boolean> {
    const _url = `/api/admin/GenStep/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * 获取详情
   * @param id 
   */
  getDetail(id: string): Observable<GenStepDetailDto> {
    const _url = `/api/admin/GenStep/${id}`;
    return this.request<GenStepDetailDto>('get', _url);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/admin/GenStep/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
