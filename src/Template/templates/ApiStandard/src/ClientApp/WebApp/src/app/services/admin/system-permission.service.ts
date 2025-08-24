import { Injectable } from '@angular/core';
import { SystemPermissionBaseService } from './system-permission-base.service';

/**
 * 
 */
@Injectable({providedIn: 'root' })
export class SystemPermissionService extends SystemPermissionBaseService {
  id: string | null = null;
  name: string | null = null;
}