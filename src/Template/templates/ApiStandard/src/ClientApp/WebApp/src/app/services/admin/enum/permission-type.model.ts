export enum PermissionType {
  /** 无权限 */
  None = 0,
  /** 可读 */
  Read = 1,
  /** 可审核 */
  Audit = 2,
  /** 仅添加 */
  Add = 4,
  /** 仅编辑 */
  Edit = 16,
  /** 可读写 */
  Write = 21,
  /** 读写且可审核 */
  AuditWrite = 23,

}
