import { SolutionType } from '../../enum/models/solution-type.model';
import { ProjectConfig } from '../../project/models/project-config.model';
import { EntityInfo } from '../../models/entity-info.model';
import { ApiDocInfo } from '../../api-doc-info/models/api-doc-info.model';
import { GenAction } from '../../gen-action/models/gen-action.model';
import { GenStep } from '../../gen-action/models/gen-step.model';
export interface Project {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  name: string;
  displayName: string;
  path: string;
  version?: string | null;
  solutionType?: SolutionType | null;
  config?: ProjectConfig | null;
  entityInfos?: EntityInfo[];
  apiDocInfos?: ApiDocInfo[];
  genActions?: GenAction[];
  genSteps?: GenStep[];

}
