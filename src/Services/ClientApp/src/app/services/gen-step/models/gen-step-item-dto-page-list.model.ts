import { GenStepItemDto } from '../../gen-action/models/gen-step-item-dto.model';
export interface GenStepItemDtoPageList {
  count: number;
  data?: GenStepItemDto[];
  pageIndex: number;

}
