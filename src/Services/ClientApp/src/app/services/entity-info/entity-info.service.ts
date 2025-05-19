import { Injectable } from '@angular/core';
import { EntityInfoBaseService } from './entity-info-base.service';

/**
 * 实体
 */
@Injectable({providedIn: 'root' })
export class EntityInfoService extends EntityInfoBaseService {
  id: string | null = null;
  name: string | null = null;
}