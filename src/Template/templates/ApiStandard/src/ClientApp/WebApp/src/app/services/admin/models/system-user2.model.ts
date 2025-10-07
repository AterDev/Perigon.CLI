import { SystemRole } from './system-role.model';
import { SystemLogs } from './system-logs.model';
import { SystemOrganization } from './system-organization.model';
import { Sex } from '../enum/sex.model';
export interface SystemUser2 {
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
/**  */  lastPwdEditTime: string;
/**  */  retryCount: number;
/**  */  avatar?: string | null;
/**  */  systemRoles: SystemRole[];
/**  */  systemLogs: SystemLogs[];
/**  */  systemOrganizations: SystemOrganization[];
/**  */  sex: Sex;
/**  */  id: string;
/**  */  createdTime: string;
/**  */  updatedTime: string;
/**  */  isDeleted: boolean;
/**  */  tenantId?: string | null;

}
