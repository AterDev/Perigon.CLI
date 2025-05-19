import { GenSourceType } from '../../enum/models/gen-source-type.model';
import { ActionStatus } from '../../enum/models/action-status.model';
/**
 * 生成操作筛选条件
 */
export interface GenActionFilterDto {
  pageIndex: number;
  pageSize: number;
  orderBy?: any | null;
  /**
   * action name
   */
  name?: string | null;
  sourceType?: GenSourceType | null;
  projectId?: string | null;
  actionStatus?: ActionStatus | null;

}
