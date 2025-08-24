import { SystemPermission2 } from './system-permission2.model';
import { SystemRole } from './system-role.model';
export interface SystemPermissionGroup2 {
/**  */  name: string;
/**  */  description?: string | null;
/**  */  permissions: SystemPermission2[];
/**  */  roles: SystemRole[];
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
