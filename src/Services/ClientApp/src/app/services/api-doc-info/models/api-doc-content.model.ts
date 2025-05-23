import { RestApiGroup } from '../../models/rest-api-group.model';
import { TypeMeta } from '../../models/type-meta.model';
import { ApiDocTag } from '../../models/api-doc-tag.model';
export interface ApiDocContent {
  restApiGroups?: RestApiGroup[];
  typeMeta?: TypeMeta[];
  openApiTags?: ApiDocTag[];

}
