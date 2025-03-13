import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { ApiDocInfoAddDto } from './models/api-doc-info-add-dto.model';
import { ApiDocInfoUpdateDto } from './models/api-doc-info-update-dto.model';
import { ApiDocInfoItemDto } from './models/api-doc-info-item-dto.model';
import { ApiDocContent } from './models/api-doc-content.model';
import { RequestLibType } from '../enum/models/request-lib-type.model';

/**
 * api文档
 */
@Injectable({ providedIn: 'root' })
export class ApiDocInfoBaseService extends BaseService {
  /**
   * 获取项目文档
   */
  list(): Observable<ApiDocInfoItemDto[]> {
    const _url = `/api/admin/ApiDocInfo`;
    return this.request<ApiDocInfoItemDto[]>('get', _url);
  }

  /**
   * 添加
   * @param data ApiDocInfoAddDto
   */
  add(data: ApiDocInfoAddDto): Observable<string> {
    const _url = `/api/admin/ApiDocInfo`;
    return this.request<string>('post', _url, data);
  }

  /**
   * 获取某个文档信息
   * @param id 
   * @param isFresh 
   */
  getApiDocContent(id: string, isFresh: boolean | null): Observable<ApiDocContent> {
    const _url = `/api/admin/ApiDocInfo/${id}?isFresh=${isFresh ?? ''}`;
    return this.request<ApiDocContent>('get', _url);
  }

  /**
   * 更新
   * @param id 
   * @param data ApiDocInfoUpdateDto
   */
  update(id: string, data: ApiDocInfoUpdateDto): Observable<boolean> {
    const _url = `/api/admin/ApiDocInfo/${id}`;
    return this.request<boolean>('put', _url, data);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/admin/ApiDocInfo/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * 导出markdown文档
   * @param id 
   */
  export(id: string): Observable<Blob> {
    const _url = `/api/admin/ApiDocInfo/export/${id}`;
    return this.downloadFile('get', _url);
  }

  /**
   * 生成前端请求
   * @param id 
   * @param webPath 
   * @param type 
   * @param swaggerPath 
   */
  generateRequest(id: string, webPath: string | null, type: RequestLibType | null, swaggerPath: string | null): Observable<boolean> {
    const _url = `/api/admin/ApiDocInfo/generateRequest/${id}?webPath=${webPath ?? ''}&type=${type ?? ''}&swaggerPath=${swaggerPath ?? ''}`;
    return this.request<boolean>('get', _url);
  }

  /**
   * 生成Csharp请求
   * @param id 
   * @param webPath 
   * @param swaggerPath 
   */
  generateCsharpRequest(id: string, webPath: string | null, swaggerPath: string | null): Observable<boolean> {
    const _url = `/api/admin/ApiDocInfo/generateCsharpRequest/${id}?webPath=${webPath ?? ''}&swaggerPath=${swaggerPath ?? ''}`;
    return this.request<boolean>('get', _url);
  }

}
