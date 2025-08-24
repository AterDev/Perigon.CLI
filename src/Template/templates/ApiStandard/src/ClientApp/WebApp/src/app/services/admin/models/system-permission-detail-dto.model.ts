import { PermissionType } from '../enum/permission-type.model';
import { SystemPermissionGroup2 } from './system-permission-group2.model';
export interface SystemPermissionDetailDto {
/**  */  id: string;
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;
/**  */  group: SystemPermissionGroup2;

}
