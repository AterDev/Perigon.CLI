import { EntityKeyType } from '../enum/models/entity-key-type.model';
import { PropertyInfo } from '../models/property-info.model';
/**
 * 模型信息
 */
export interface ModelInfo {
  /**
   * 类名
   */
  name: string;
  /**
   * 类注释
   */
  comment?: string | null;
  /**
   * 类注释
   */
  summary?: string | null;
  keyType?: EntityKeyType | null;
  /**
   * 是否为枚举类
   */
  isEnum?: boolean | null;
  isList: boolean;
  /**
   * 属性
   */
  propertyInfos?: PropertyInfo[];

}
