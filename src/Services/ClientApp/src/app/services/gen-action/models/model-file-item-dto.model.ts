/**
 * 模型文件项
 */
export interface ModelFileItemDto {
  /**
   * 名称
   */
  name: string;
  /**
   * 路径
   */
  fullName: string;
  content?: string | null;

}
