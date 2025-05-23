import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step筛选条件
 */
export interface GenStepFilterDto {
  pageIndex: number;
  pageSize: number;
  orderBy?: any | null;
  name?: string | null;
  genStepType?: GenStepType | null;
  projectId?: string | null;
  fileType?: string | null;

}
