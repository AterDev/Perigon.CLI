import { PermissionType } from '../enum/permission-type.model';
export interface SystemPermissionItemDto {
/**  */  id: string;
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;

}
