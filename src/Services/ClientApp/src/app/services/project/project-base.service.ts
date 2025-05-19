import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { ProjectConfig } from './models/project-config.model';
import { Project } from './models/project.model';

/**
 * 项目
 */
@Injectable({ providedIn: 'root' })
export class ProjectBaseService extends BaseService {
  /**
   * 获取解决方案列表
   */
  list(): Observable<Project[]> {
    const _url = `/api/admin/Project`;
    return this.request<Project[]>('get', _url);
  }

  /**
   * 添加项目
   * @param name 
   * @param path 
   */
  add(name: string | null, path: string | null): Observable<string> {
    const _url = `/api/admin/Project?name=${name ?? ''}&path=${path ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 获取工具版本
   */
  getVersion(): Observable<string> {
    const _url = `/api/admin/Project/version`;
    return this.request<string>('get', _url);
  }

  /**
   * 详情
   * @param id 
   */
  project(id: string): Observable<Project> {
    const _url = `/api/admin/Project/${id}`;
    return this.request<Project>('get', _url);
  }

  /**
   * 删除项目
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/admin/Project/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * 添加微服务
   * @param name 
   */
  addService(name: string | null): Observable<boolean> {
    const _url = `/api/admin/Project/service?name=${name ?? ''}`;
    return this.request<boolean>('post', _url);
  }

  /**
   * 更新解决方案
   */
  updateSolution(): Observable<string> {
    const _url = `/api/admin/Project/solution`;
    return this.request<string>('put', _url);
  }

  /**
   * 打开解决方案，仅支持sln
   * @param path 
   */
  openSolution(path: string | null): Observable<string> {
    const _url = `/api/admin/Project/open?path=${path ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 更新配置
   * @param id 
   * @param data ProjectConfig
   */
  updateConfig(id: string, data: ProjectConfig): Observable<boolean> {
    const _url = `/api/admin/Project/setting/${id}`;
    return this.request<boolean>('put', _url, data);
  }

}
