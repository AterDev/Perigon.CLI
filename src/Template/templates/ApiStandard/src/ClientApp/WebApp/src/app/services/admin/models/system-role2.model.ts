import { SystemUser2 } from './system-user2.model';
import { SystemPermissionGroup2 } from './system-permission-group2.model';
import { SystemMenu } from './system-menu.model';
export interface SystemRole2 {
/**  */  name: string;
/**  */  nameValue: string;
/**  */  isSystem: boolean;
/**  */  icon?: string | null;
/**  */  users: SystemUser2[];
/**  */  permissionGroups: SystemPermissionGroup2[];
/**  */  menus: SystemMenu[];
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
