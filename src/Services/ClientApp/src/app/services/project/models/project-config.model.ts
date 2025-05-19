import { ControllerType } from '../../enum/models/controller-type.model';
export interface ProjectConfig {
  idType: string;
  createdTimeName: string;
  updatedTimeName: string;
  version: string;
  sharePath: string;
  entityPath: string;
  entityFrameworkPath: string;
  applicationPath: string;
  apiPath: string;
  microservicePath: string;
  solutionPath: string;
  isLight: boolean;
  controllerType?: ControllerType | null;
  isSplitController?: boolean | null;

}
