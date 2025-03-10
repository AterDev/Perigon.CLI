import { RestApiGroup } from '../../models/rest-api-group.model';
import { ModelInfo } from '../../models/model-info.model';
import { ApiDocTag } from '../../models/api-doc-tag.model';
export interface ApiDocContent {
  restApiGroups?: RestApiGroup[];
  modelInfos?: ModelInfo[];
  openApiTags?: ApiDocTag[];

}
