import { MenuType } from '../enum/menu-type.model';
export interface SystemMenuAddDto {
/**  */  name: string;
/**  */  path?: string | null;
/**  */  icon?: string | null;
/**  */  parentId?: string | null;
/**  */  isValid: boolean;
/**  */  accessCode: string;
/**  */  menuType: MenuType;
/**  */  sort: number;
/**  */  hidden: boolean;

}
