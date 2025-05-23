/**
 * 接口文档更新时请求结构
 */
export interface ApiDocInfoUpdateDto {
  /**
   * 文档名称
   */
  name?: string | null;
  /**
   * 文档描述
   */
  description?: string | null;
  /**
   * 文档地址
   */
  path?: string | null;
  /**
   * 生成路径
   */
  localPath?: string | null;

}
