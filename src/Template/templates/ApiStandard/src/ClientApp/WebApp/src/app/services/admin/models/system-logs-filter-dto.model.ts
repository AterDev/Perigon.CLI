import { UserActionType } from '../enum/user-action-type.model';
export interface SystemLogsFilterDto {
/**  */  actionUserName?: string | null;
/**  */  targetName?: string | null;
/**  */  actionType?: UserActionType | null;
/**  */  startDate?: string | null;
/**  */  endDate?: string | null;
/**  */  pageIndex?: number | null;
/**  */  pageSize?: number | null;
/**  */  orderBy?: Map<string, boolean> | null;

}
