import { UserActionType } from '../enum/user-action-type.model';
import { SystemUser2 } from './system-user2.model';
export interface SystemLogs {
/**  */  actionUserName: string;
/**  */  targetName?: string | null;
/**  */  route: string;
/**  */  actionType: UserActionType;
/**  */  description?: string | null;
/**  */  systemUser: SystemUser2;
/**  */  systemUserId: string;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
