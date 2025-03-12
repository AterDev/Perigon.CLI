import { Variable } from '../../models/variable.model';
import { TypeMeta } from '../../models/type-meta.model';
/**
 * 操作执行模型
 */
export interface GenActionRunDto {
  /**
   * 操作id
   */
  id: string;
  /**
   * 源文件路径
   */
  sourceFilePath?: string | null;
  /**
   * 仅输出
   */
  onlyOutput: boolean;
  /**
   * 额外变量
   */
  variables?: Variable[] | null;
  /**
   * 模型信息
   */
  modelInfo?: TypeMeta | null;

}
