import { GenStepType } from '../enum/models/gen-step-type.model';
import { GenAction } from '../gen-action/models/gen-action.model';
import { Project } from '../project/models/project.model';
export interface GenStep {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  templateContent?: string | null;
  path?: string | null;
  command?: string | null;
  genStepType?: GenStepType | null;
  genActions?: GenAction[];
  project?: Project | null;
  projectId: string;

}
