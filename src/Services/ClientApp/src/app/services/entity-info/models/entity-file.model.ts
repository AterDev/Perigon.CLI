export interface EntityFile {
  name: string;
  /**
   * 注释说明
   */
  comment?: string | null;
  baseDirPath: string;
  fullName: string;
  content?: string | null;
  /**
   * 所属模块
   */
  moduleName?: string | null;
  hasDto: boolean;
  hasManager: boolean;
  hasAPI: boolean;

}
