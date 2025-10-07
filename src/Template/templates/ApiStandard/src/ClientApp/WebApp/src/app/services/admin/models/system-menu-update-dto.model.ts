import { MenuType } from '../enum/menu-type.model';
export interface SystemMenuUpdateDto {
/**  */  name?: string | null;
/**  */  path?: string | null;
/**  */  icon?: string | null;
/**  */  isValid?: boolean | null;
/**  */  accessCode?: string | null;
/**  */  menuType?: MenuType | null;
/**  */  sort?: number | null;
/**  */  hidden?: boolean | null;

}
