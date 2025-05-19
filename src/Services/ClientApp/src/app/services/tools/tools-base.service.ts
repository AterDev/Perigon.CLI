import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { ConvertDto } from './models/convert-dto.model';
import { StringConvertType } from '../enum/models/string-convert-type.model';

/**
 * Tools
 */
@Injectable({ providedIn: 'root' })
export class ToolsBaseService extends BaseService {
  /**
   * 转换成类
   * @param data ConvertDto
   */
  convertToClass(data: ConvertDto): Observable<string[]> {
    const _url = `/api/Tools/classModel`;
    return this.request<string[]>('post', _url, data);
  }

  /**
   * 字符串处理
   * @param content 
   * @param type 
   */
  convertString(content: string | null, type: StringConvertType | null): Observable<Map<string, string>> {
    const _url = `/api/Tools/string?content=${content ?? ''}&type=${type ?? ''}`;
    return this.request<Map<string, string>>('get', _url);
  }

}
