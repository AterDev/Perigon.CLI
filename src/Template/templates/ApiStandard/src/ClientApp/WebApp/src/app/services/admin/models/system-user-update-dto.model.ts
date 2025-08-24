import { GenderType } from '../enum/gender-type.model';
export interface SystemUserUpdateDto {
/**  */  userName?: string | null;
/**  */  password?: string | null;
/**  */  realName?: string | null;
/**  */  email?: string | null;
/**  */  phoneNumber?: string | null;
/**  */  avatar?: string | null;
/**  */  sex?: GenderType | null;
/**  */  roleIds?: String[] | null;

}
