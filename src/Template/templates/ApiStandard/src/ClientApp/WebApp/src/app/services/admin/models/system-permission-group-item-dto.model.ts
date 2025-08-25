import { SystemPermission } from './system-permission.model';
export interface SystemPermissionGroupItemDto {
/**  */  id: string;
/**  */  name: string;
/**  */  description?: string | null;
/**  */  permissions?: SystemPermission[] | null;

}
