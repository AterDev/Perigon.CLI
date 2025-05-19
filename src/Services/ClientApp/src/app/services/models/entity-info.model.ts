import { EntityKeyType } from '../enum/models/entity-key-type.model';
import { Project } from '../project/models/project.model';
import { PropertyInfo } from '../models/property-info.model';
export interface EntityInfo {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  md5Hash: string;
  moduleName?: string | null;
  filePath: string;
  name: string;
  namespaceName: string;
  assemblyName?: string | null;
  comment?: string | null;
  summary?: string | null;
  keyType?: EntityKeyType | null;
  isEnum?: boolean | null;
  isList: boolean;
  project?: Project | null;
  projectId: string;
  propertyInfos?: PropertyInfo[];

}
