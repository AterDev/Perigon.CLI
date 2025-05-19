import { DBType } from '../../enum/models/dbtype.model';
import { CacheType } from '../../enum/models/cache-type.model';
import { FrontType } from '../../enum/models/front-type.model';
import { ProjectType } from '../../enum/models/project-type.model';
export interface CreateSolutionDto {
  name: string;
  path: string;
  isLight: boolean;
  dbType?: DBType | null;
  cacheType?: CacheType | null;
  frontType?: FrontType | null;
  hasTenant: boolean;
  hasIdentityServer: boolean;
  hasTaskManager: boolean;
  commandDbConnStrings?: string | null;
  queryDbConnStrings?: string | null;
  cacheConnStrings?: string | null;
  cacheInstanceName?: string | null;
  projectType?: ProjectType | null;
  defaultPassword?: string | null;
  modules: string[];

}
