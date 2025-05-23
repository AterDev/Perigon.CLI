import { Project } from '../../project/models/project.model';
export interface ApiDocInfo {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  name: string;
  description?: string | null;
  path: string;
  localPath?: string | null;
  content?: string | null;
  project?: Project | null;
  projectId: string;

}
