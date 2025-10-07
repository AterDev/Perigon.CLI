import { inject, Injectable } from "@angular/core";
import { SystemConfigService } from './system-config.service';
import { SystemLogsService } from './system-logs.service';
import { SystemMenuService } from './system-menu.service';
import { SystemPermissionService } from './system-permission.service';
import { SystemPermissionGroupService } from './system-permission-group.service';
import { SystemRoleService } from './system-role.service';
import { SystemUserService } from './system-user.service';
@Injectable({
  providedIn: 'root'
})
export class AdminClient {
  public systemConfig = inject(SystemConfigService);
  public systemLogs = inject(SystemLogsService);
  public systemMenu = inject(SystemMenuService);
  public systemPermission = inject(SystemPermissionService);
  public systemPermissionGroup = inject(SystemPermissionGroupService);
  public systemRole = inject(SystemRoleService);
  public systemUser = inject(SystemUserService);
}
