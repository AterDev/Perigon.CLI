import { GenderType } from '../enum/gender-type.model';
export interface SystemUserDetailDto {
/**  */  userName: string;
/**  */  realName?: string | null;
/**  */  email?: string | null;
/**  */  emailConfirmed: boolean;
/**  */  phoneNumber?: string | null;
/**  */  phoneNumberConfirmed: boolean;
/**  */  twoFactorEnabled: boolean;
/**  */  lockoutEnd?: string | null;
/**  */  lockoutEnabled: boolean;
/**  */  accessFailedCount: number;
/**  */  lastLoginTime?: string | null;
/**  */  retryCount: number;
/**  */  avatar?: string | null;
/**  */  sex: GenderType;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;

}
