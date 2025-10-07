import { PermissionType } from '../enum/permission-type.model';
export interface SystemPermissionAddDto {
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;
/**  */  systemPermissionGroupId: string;

}
