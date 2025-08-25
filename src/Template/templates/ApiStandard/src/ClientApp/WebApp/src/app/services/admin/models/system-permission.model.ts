import { PermissionType } from '../enum/permission-type.model';
import { SystemPermissionGroup } from './system-permission-group.model';
export interface SystemPermission {
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;
/**  */  group: SystemPermissionGroup;
/**  */  groupId: string;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
