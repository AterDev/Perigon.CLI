/**
 * 接口文档列表元素
 */
export interface ApiDocInfoItemDto {
  /**
   * 文档名称
   */
  name: string;
  /**
   * 文档地址
   */
  path: string;
  /**
   * 文档描述
   */
  description?: string | null;
  /**
   * 生成路径
   */
  localPath?: string | null;
  id: string;
  createdTime: Date;

}
