import { Injectable } from '@angular/core';
import { ProjectBaseService } from './project-base.service';

/**
 * 项目
 */
@Injectable({providedIn: 'root' })
export class ProjectService extends ProjectBaseService {
  id: string | null = null;
  name: string | null = null;
}