import { Variable } from '../../models/variable.model';
import { GenSourceType } from '../../enum/models/gen-source-type.model';
import { ActionStatus } from '../../enum/models/action-status.model';
/**
 * 生成操作添加时DTO
 */
export interface GenActionAddDto {
  /**
   * action name
   */
  name: string;
  description?: string | null;
  /**
   * 实体路径
   */
  entityPath?: string | null;
  /**
   * open api path
   */
  openApiPath?: string | null;
  variables?: Variable[];
  sourceType?: GenSourceType | null;
  actionStatus?: ActionStatus | null;

}
