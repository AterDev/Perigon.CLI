import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { ConfigData } from './models/config-data.model';

/**
 * 高级功能
 */
@Injectable({ providedIn: 'root' })
export class AdvanceBaseService extends BaseService {
  /**
   * 获取配置
   * @param key 
   */
  getConfig(key: string | null): Observable<ConfigData> {
    const _url = `/api/admin/Advance/config?key=${key ?? ''}`;
    return this.request<ConfigData>('get', _url);
  }

  /**
   * 设置配置
   * @param key 
   * @param value 
   */
  setConfig(key: string | null, value: string | null): Observable<any> {
    const _url = `/api/admin/Advance/config?key=${key ?? ''}&value=${value ?? ''}`;
    return this.request<any>('put', _url);
  }

  /**
   * 测试AI对话
   * @param prompt 
   */
  chat(prompt: string | null): Observable<any> {
    const _url = `/api/admin/Advance/chat?prompt=${prompt ?? ''}`;
    return this.request<any>('get', _url);
  }

  /**
   * 清除对话
   */
  clearChat(): Observable<any> {
    const _url = `/api/admin/Advance/chat`;
    return this.request<any>('delete', _url);
  }

}
