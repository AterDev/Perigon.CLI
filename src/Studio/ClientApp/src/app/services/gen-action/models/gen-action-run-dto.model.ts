import { Variable } from '../../models/variable.model';
import { ModelInfo } from '../../models/model-info.model';
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
  modelInfo?: ModelInfo | null;

}
