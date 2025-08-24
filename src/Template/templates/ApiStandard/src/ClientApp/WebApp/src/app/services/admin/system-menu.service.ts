import { Injectable } from '@angular/core';
import { SystemMenuBaseService } from './system-menu-base.service';

/**
 * 
 */
@Injectable({providedIn: 'root' })
export class SystemMenuService extends SystemMenuBaseService {
  id: string | null = null;
  name: string | null = null;
}