/**
 * 接口文档添加时请求结构
 */
export interface ApiDocInfoAddDto {
  /**
   * 文档名称
   */
  name: string;
  /**
   * 文档描述
   */
  description?: string | null;
  /**
   * 文档地址
   */
  path: string;
  /**
   * 生成路径
   */
  localPath?: string | null;

}
