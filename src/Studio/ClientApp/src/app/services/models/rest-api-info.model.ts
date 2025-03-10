import { OperationType } from '../enum/models/operation-type.model';
import { PropertyInfo } from '../models/property-info.model';
import { ModelInfo } from '../models/model-info.model';
/**
 * 接口信息
 */
export interface RestApiInfo {
  operationType?: OperationType | null;
  /**
   * 路由
   */
  router: string;
  /**
   * 说明
   */
  summary?: string | null;
  tag?: string | null;
  operationId: string;
  /**
   * 请求查询参数
   */
  queryParameters?: PropertyInfo[] | null;
  /**
   * 模型信息
   */
  requestInfo?: ModelInfo | null;
  /**
   * 模型信息
   */
  responseInfo?: ModelInfo | null;

}
