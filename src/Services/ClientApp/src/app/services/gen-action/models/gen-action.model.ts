import { Variable } from '../../models/variable.model';
import { GenSourceType } from '../../enum/models/gen-source-type.model';
import { GenStep } from '../../gen-action/models/gen-step.model';
import { Project } from '../../project/models/project.model';
import { ActionStatus } from '../../enum/models/action-status.model';
export interface GenAction {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  name: string;
  description?: string | null;
  entityPath?: string | null;
  openApiPath?: string | null;
  variables?: Variable[];
  sourceType?: GenSourceType | null;
  genSteps?: GenStep[];
  project?: Project | null;
  projectId: string;
  actionStatus?: ActionStatus | null;

}
