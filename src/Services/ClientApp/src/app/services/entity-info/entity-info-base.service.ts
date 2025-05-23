import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { UpdateDtoDto } from './models/update-dto-dto.model';
import { GenerateDto } from './models/generate-dto.model';
import { EntityFile } from './models/entity-file.model';

/**
 * 实体
 */
@Injectable({ providedIn: 'root' })
export class EntityInfoBaseService extends BaseService {
  /**
   * 实体列表
   * @param id 
   */
  list(id: string): Observable<EntityFile[]> {
    const _url = `/api/admin/EntityInfo/${id}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * s
            获取dtos
   * @param entityFilePath 
   */
  getDtos(entityFilePath: string | null): Observable<EntityFile[]> {
    const _url = `/api/admin/EntityInfo/dtos?entityFilePath=${entityFilePath ?? ''}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * 清理解决方案
   */
  cleanSolution(): Observable<string> {
    const _url = `/api/admin/EntityInfo`;
    return this.request<string>('delete', _url);
  }

  /**
   * 获取文件内容 entity/manager
   * @param entityName 
   * @param isManager 是否为manager
   * @param moduleName 
   */
  getFileContent(entityName: string | null, isManager: boolean | null, moduleName: string | null): Observable<EntityFile> {
    const _url = `/api/admin/EntityInfo/fileContent?entityName=${entityName ?? ''}&isManager=${isManager ?? ''}&moduleName=${moduleName ?? ''}`;
    return this.request<EntityFile>('get', _url);
  }

  /**
   * 更新内容
   * @param data UpdateDtoDto
   */
  updateDtoContent(data: UpdateDtoDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/dto`;
    return this.request<boolean>('put', _url, data);
  }

  /**
   * 代码生成
   * @param data GenerateDto
   */
  generate(data: GenerateDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/generate`;
    return this.request<boolean>('post', _url, data);
  }

}
