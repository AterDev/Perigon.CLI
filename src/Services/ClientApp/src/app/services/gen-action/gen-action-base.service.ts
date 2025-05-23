import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { GenActionFilterDto } from './models/gen-action-filter-dto.model';
import { GenActionAddDto } from './models/gen-action-add-dto.model';
import { GenActionUpdateDto } from './models/gen-action-update-dto.model';
import { GenActionRunDto } from './models/gen-action-run-dto.model';
import { GenActionItemDtoPageList } from './models/gen-action-item-dto-page-list.model';
import { GenStepItemDto } from './models/gen-step-item-dto.model';
import { ModelFileItemDto } from './models/model-file-item-dto.model';
import { GenActionDetailDto } from './models/gen-action-detail-dto.model';
import { GenActionResultDto } from './models/gen-action-result-dto.model';
import { GenSourceType } from '../enum/models/gen-source-type.model';

/**
 * 生成操作
 */
@Injectable({ providedIn: 'root' })
export class GenActionBaseService extends BaseService {
  /**
   * 分页数据
   * @param data GenActionFilterDto
   */
  filter(data: GenActionFilterDto): Observable<GenActionItemDtoPageList> {
    const _url = `/api/admin/GenAction/filter`;
    return this.request<GenActionItemDtoPageList>('post', _url, data);
  }

  /**
   * 获取操作步骤
   * @param id 
   */
  getSteps(id: string): Observable<GenStepItemDto[]> {
    const _url = `/api/admin/GenAction/steps/${id}`;
    return this.request<GenStepItemDto[]>('get', _url);
  }

  /**
   * 添加操作步骤
   * @param id 
   * @param data string[]
   */
  addSteps(id: string, data: string[]): Observable<boolean> {
    const _url = `/api/admin/GenAction/steps/${id}`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 获取模型列表
   * @param sourceType 
   */
  getModelFile(sourceType: GenSourceType | null): Observable<ModelFileItemDto[]> {
    const _url = `/api/admin/GenAction/modelFile?sourceType=${sourceType ?? ''}`;
    return this.request<ModelFileItemDto[]>('get', _url);
  }

  /**
   * 新增
   * @param data GenActionAddDto
   */
  add(data: GenActionAddDto): Observable<string> {
    const _url = `/api/admin/GenAction`;
    return this.request<string>('post', _url, data);
  }

  /**
   * 更新数据
   * @param id 
   * @param data GenActionUpdateDto
   */
  update(id: string, data: GenActionUpdateDto): Observable<boolean> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * 获取详情
   * @param id 
   */
  getDetail(id: string): Observable<GenActionDetailDto> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<GenActionDetailDto>('get', _url);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * 执行操作
   * @param data GenActionRunDto
   */
  execute(data: GenActionRunDto): Observable<GenActionResultDto> {
    const _url = `/api/admin/GenAction/execute`;
    return this.request<GenActionResultDto>('post', _url, data);
  }

  /**
   * 从本地导入
   */
  sync(): Observable<string> {
    const _url = `/api/admin/GenAction/syncTemplate`;
    return this.request<string>('get', _url);
  }

  /**
   * 保存配置
   */
  saveSyncData(): Observable<boolean> {
    const _url = `/api/admin/GenAction/saveTemplate`;
    return this.request<boolean>('get', _url);
  }

}
