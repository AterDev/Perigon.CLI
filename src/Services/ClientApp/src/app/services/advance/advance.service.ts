import { Injectable } from '@angular/core';
import { AdvanceBaseService } from './advance-base.service';

/**
 * 高级功能
 */
@Injectable({providedIn: 'root' })
export class AdvanceService extends AdvanceBaseService {
  id: string | null = null;
  name: string | null = null;
}