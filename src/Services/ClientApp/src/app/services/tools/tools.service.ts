import { Injectable } from '@angular/core';
import { ToolsBaseService } from './tools-base.service';

/**
 * Tools
 */
@Injectable({providedIn: 'root' })
export class ToolsService extends ToolsBaseService {
  id: string | null = null;
  name: string | null = null;
}