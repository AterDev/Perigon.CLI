import { PermissionType } from '../enum/permission-type.model';
import { SystemPermissionGroup2 } from './system-permission-group2.model';
export interface SystemPermission2 {
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;
/**  */  group: SystemPermissionGroup2;
/**  */  groupId: string;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
