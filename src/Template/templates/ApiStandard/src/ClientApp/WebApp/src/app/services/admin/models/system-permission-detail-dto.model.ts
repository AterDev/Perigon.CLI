import { PermissionType } from '../enum/permission-type.model';
import { SystemPermissionGroup } from './system-permission-group.model';
export interface SystemPermissionDetailDto {
/**  */  id: string;
/**  */  name: string;
/**  */  description?: string | null;
/**  */  enable: boolean;
/**  */  permissionType: PermissionType;
/**  */  group: SystemPermissionGroup;

}
