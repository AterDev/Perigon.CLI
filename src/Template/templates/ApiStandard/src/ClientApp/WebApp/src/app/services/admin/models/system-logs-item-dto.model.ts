import { UserActionType } from '../enum/user-action-type.model';
export interface SystemLogsItemDto {
/**  */  actionUserName: string;
/**  */  targetName: string;
/**  */  route: string;
/**  */  actionType: UserActionType;
/**  */  description?: string | null;
/**  */  id: string;
/**  */  createdTime: string;

}
