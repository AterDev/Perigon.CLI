import { Injectable } from '@angular/core';
import { GenStepBaseService } from './gen-step-base.service';

/**
 * task step
 */
@Injectable({providedIn: 'root' })
export class GenStepService extends GenStepBaseService {
  id: string | null = null;
  name: string | null = null;
}