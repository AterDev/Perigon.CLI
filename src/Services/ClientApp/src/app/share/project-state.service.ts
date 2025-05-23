import { Injectable, signal, Signal } from '@angular/core';
import { EntityFile } from '../services/entity-info/models/entity-file.model';
import { Project } from '../services/project/models/project.model';

@Injectable({
  providedIn: 'root'
})

export class ProjectStateService {
  project: Project | null = null;
  version: string | null = null;
  currentEntity =  signal<EntityFile|null>(null);
  constructor() {
    this.getProject();
    this.getVersion();
  }

  setVersion(version: string): void {
    localStorage.setItem('version', version);
  }
  getVersion(): string | null {
    if (this.version) {
      return this.version;
    }
    const version = localStorage.getItem('version');
    if (version)
      this.version = version;
    return this.version;
  }

  setProject(project: Project): void {
    this.project = project;
    localStorage.setItem('project', JSON.stringify(project));
    localStorage.setItem('projectId', project.id);
  }
  getProject(): Project | null {
    if (this.project) {
      return this.project;
    }
    const project = localStorage.getItem('project');
    if (project)
      this.project = JSON.parse(project);

    return this.project;
  }

}
