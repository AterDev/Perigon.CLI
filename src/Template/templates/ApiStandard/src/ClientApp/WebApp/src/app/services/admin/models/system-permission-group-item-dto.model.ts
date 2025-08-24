import { SystemPermission2 } from './system-permission2.model';
export interface SystemPermissionGroupItemDto {
/**  */  id: string;
/**  */  name: string;
/**  */  description?: string | null;
/**  */  permissions?: SystemPermission2[] | null;

}
