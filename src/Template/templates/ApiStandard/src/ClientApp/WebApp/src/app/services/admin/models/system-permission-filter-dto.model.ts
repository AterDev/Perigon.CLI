import { PermissionType } from '../enum/permission-type.model';
export interface SystemPermissionFilterDto {
/**  */  name?: string | null;
/**  */  permissionType?: PermissionType | null;
/**  */  groupId?: string | null;
/**  */  pageIndex?: number | null;
/**  */  pageSize?: number | null;
/**  */  orderBy?: Map<string, boolean> | null;

}
