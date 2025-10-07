import { GenderType } from '../enum/gender-type.model';
export interface SystemUserItemDto {
/**  */  userName: string;
/**  */  realName?: string | null;
/**  */  email?: string | null;
/**  */  lastLoginTime?: string | null;
/**  */  sex: GenderType;
/**  */  id: string;
/**  */  createdTime: string;

}
