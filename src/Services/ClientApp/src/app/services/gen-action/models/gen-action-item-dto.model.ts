import { GenSourceType } from '../../enum/models/gen-source-type.model';
import { ActionStatus } from '../../enum/models/action-status.model';
import { Variable } from '../../models/variable.model';
/**
 * 生成操作列表元素
 */
export interface GenActionItemDto {
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
  sourceType?: GenSourceType | null;
  actionStatus?: ActionStatus | null;
  id: string;
  createdTime: Date;
  variables?: Variable[];

}
