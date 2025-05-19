import { MatPaginatorIntl } from '@angular/material/paginator';

/**
 * mat 分页国际化自定义内容
 */
export class CustomPaginatorIntl extends MatPaginatorIntl {
  constructor() {
    super();
    this.itemsPerPageLabel = "每页";
    this.nextPageLabel = "下一页";
    this.previousPageLabel = "上一页";
    this.firstPageLabel = "首页";
    this.lastPageLabel = "尾页";
  }
}
