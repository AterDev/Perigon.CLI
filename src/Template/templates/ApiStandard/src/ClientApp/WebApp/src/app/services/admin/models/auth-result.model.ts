import { SystemMenu } from './system-menu.model';
export interface AuthResult {
/**  */  id: string;
/**  */  username: string;
/**  */  roles: String[];
/**  */  menus?: SystemMenu[] | null;
/**  */  accessToken: string;
/**  */  expiresIn: number;
/**  */  refreshToken: string;
/**  */  permissionGroups?: any | null;

}
