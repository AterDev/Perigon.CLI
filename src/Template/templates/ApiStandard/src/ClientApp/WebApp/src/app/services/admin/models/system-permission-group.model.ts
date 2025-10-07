import { SystemPermission } from './system-permission.model';
export interface SystemPermissionGroup {
/**  */  name: string;
/**  */  description?: string | null;
/**  */  permissions: SystemPermission[];
/**  */  roles: any;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
