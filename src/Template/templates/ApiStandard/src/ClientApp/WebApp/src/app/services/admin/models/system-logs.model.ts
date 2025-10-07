import { UserActionType } from '../enum/user-action-type.model';
import { SystemUser } from './system-user.model';
export interface SystemLogs {
/**  */  actionUserName: string;
/**  */  targetName?: string | null;
/**  */  route: string;
/**  */  actionType: UserActionType;
/**  */  description?: string | null;
/**  */  systemUser: SystemUser;
/**  */  systemUserId: string;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
