import { GenderType } from '../enum/gender-type.model';
export interface SystemUserAddDto {
/**  */  userName: string;
/**  */  password: string;
/**  */  roleIds?: String[] | null;
/**  */  realName?: string | null;
/**  */  email?: string | null;
/**  */  phoneNumber?: string | null;
/**  */  avatar?: string | null;
/**  */  sex: GenderType;

}
