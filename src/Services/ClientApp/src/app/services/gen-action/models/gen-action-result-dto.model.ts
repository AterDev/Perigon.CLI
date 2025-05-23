import { ModelFileItemDto } from '../../gen-action/models/model-file-item-dto.model';
/**
 * 执行结果
 */
export interface GenActionResultDto {
  /**
   * 是否成功
   */
  isSuccess: boolean;
  /**
   * 错误信息
   */
  errorMsg?: string | null;
  outputFiles?: ModelFileItemDto[];

}
