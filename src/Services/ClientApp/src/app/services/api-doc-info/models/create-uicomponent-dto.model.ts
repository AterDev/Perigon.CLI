import { UIType } from '../../enum/models/uitype.model';
import { ComponentType } from '../../enum/models/component-type.model';
import { ModelInfo } from '../../models/model-info.model';
export interface CreateUIComponentDto {
  uiType?: UIType | null;
  componentType?: ComponentType | null;
  /**
   * 模型信息
   */
  modelInfo?: ModelInfo | null;
  serviceName: string;

}
