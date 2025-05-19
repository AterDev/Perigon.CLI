import { ModelInfo } from '../models/model-info.model';
export interface ModelProperty {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  type: string;
  name: string;
  displayName?: string | null;
  isList: boolean;
  isPublic: boolean;
  isNavigation: boolean;
  isJsonIgnore: boolean;
  navigationName?: string | null;
  isComplexType: boolean;
  hasMany?: boolean | null;
  isEnum: boolean;
  hasSet: boolean;
  attributeText?: string | null;
  commentXml?: string | null;
  commentSummary?: string | null;
  isRequired: boolean;
  isNullable: boolean;
  minLength?: number | null;
  maxLength?: number | null;
  isDecimal: boolean;
  suffixContent?: string | null;
  defaultValue: string;
  entityInfo?: ModelInfo | null;
  modelInfoId: string;

}
