import { SystemUser } from './system-user.model';
import { SystemPermissionGroup } from './system-permission-group.model';
export interface SystemRole {
/**  */  name: string;
/**  */  nameValue: string;
/**  */  isSystem: boolean;
/**  */  icon?: string | null;
/**  */  users: SystemUser[];
/**  */  permissionGroups: SystemPermissionGroup[];
/**  */  menus: any;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
