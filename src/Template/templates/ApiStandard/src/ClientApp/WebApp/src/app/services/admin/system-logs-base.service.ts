import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemLogsFilterDto } from './models/system-logs-filter-dto.model';
import { PageListOfSystemLogsItemDto } from './models/page-list-of-system-logs-item-dto.model';

/**
 * 
 */
export class SystemLogsBaseService extends BaseService {
  /**
   * filter
   * @param data SystemLogsFilterDto
   */
  filter(data: SystemLogsFilterDto): Observable<PageListOfSystemLogsItemDto> {
    const _url = `/api/SystemLogs/filter`;
    return this.request<PageListOfSystemLogsItemDto>('post', _url, data);
  }

}
