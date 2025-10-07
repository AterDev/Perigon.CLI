import { PermissionType } from '../enum/permission-type.model';
export interface SystemPermissionUpdateDto {
/**  */  name?: string | null;
/**  */  description?: string | null;
/**  */  enable?: boolean | null;
/**  */  permissionType?: PermissionType | null;
/**  */  systemPermissionGroupId?: string | null;

}
