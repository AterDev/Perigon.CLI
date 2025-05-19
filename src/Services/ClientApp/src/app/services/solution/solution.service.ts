import { Injectable } from '@angular/core';
import { SolutionBaseService } from './solution-base.service';

/**
 * 功能模块
 */
@Injectable({providedIn: 'root' })
export class SolutionService extends SolutionBaseService {
  id: string | null = null;
  name: string | null = null;
}