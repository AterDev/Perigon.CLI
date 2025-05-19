import { ProjectType } from '../../enum/models/project-type.model';
export interface SubProjectInfo {
  name: string;
  path: string;
  projectType?: ProjectType | null;

}
