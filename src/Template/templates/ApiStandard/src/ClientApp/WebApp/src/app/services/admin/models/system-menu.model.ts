import { SystemRole } from './system-role.model';
import { MenuType } from '../enum/menu-type.model';
export interface SystemMenu {
/**  */  name: string;
/**  */  path?: string | null;
/**  */  icon?: string | null;
/**  */  parent: SystemMenu;
/**  */  parentId?: string | null;
/**  */  isValid: boolean;
/**  */  children: any;
/**  */  roles: SystemRole[];
/**  */  accessCode: string;
/**  */  menuType: MenuType;
/**  */  sort: number;
/**  */  hidden: boolean;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
