import { GenActionItemDto } from '../../gen-action/models/gen-action-item-dto.model';
export interface GenActionItemDtoPageList {
  count: number;
  data?: GenActionItemDto[];
  pageIndex: number;

}
