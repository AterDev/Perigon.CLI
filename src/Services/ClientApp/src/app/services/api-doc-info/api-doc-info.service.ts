import { Injectable } from '@angular/core';
import { ApiDocInfoBaseService } from './api-doc-info-base.service';

/**
 * api文档
 */
@Injectable({providedIn: 'root' })
export class ApiDocInfoService extends ApiDocInfoBaseService {
  id: string | null = null;
  name: string | null = null;
}