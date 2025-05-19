import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step添加时DTO
 */
export interface GenStepAddDto {
  /**
   * 步骤名称
   */
  name: string;
  /**
   * 模板或命令内容
   */
  content?: string | null;
  /**
   * 生成内容
   */
  outputContent?: string | null;
  /**
   * 模板或脚本路径
   */
  path?: string | null;
  /**
   * 输出路径
   */
  outputPath?: string | null;
  genStepType?: GenStepType | null;

}
