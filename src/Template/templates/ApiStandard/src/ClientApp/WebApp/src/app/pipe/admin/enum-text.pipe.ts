// <auto-generate>
import { Injectable, Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
@Injectable({ providedIn: 'root' })
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): string {
    let result = '';
    switch (type) {
      case 'GenderType':
        {
          switch (value) {
            case 0: result = '男性'; break;
            case 1: result = '女性'; break;
            case 2: result = '其他'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'MenuType':
        {
          switch (value) {
            case 0: result = '页面'; break;
            case 1: result = '按钮'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'PermissionType':
        {
          switch (value) {
            case 0: result = '无权限'; break;
            case 1: result = '可读'; break;
            case 2: result = '可审核'; break;
            case 4: result = '仅添加'; break;
            case 16: result = '仅编辑'; break;
            case 21: result = '可读写'; break;
            case 23: result = '读写且可审核'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'Sex':
        {
          switch (value) {
            case 0: result = '男性'; break;
            case 1: result = '女性'; break;
            case 2: result = '其他'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'UserActionType':
        {
          switch (value) {
            case 0: result = '其它'; break;
            case 1: result = '登录'; break;
            case 2: result = '添加'; break;
            case 3: result = '更新'; break;
            case 4: result = '删除'; break;
            case 5: result = '审核'; break;
            case 6: result = '导入'; break;
            case 7: result = '导出'; break;
            default: result = '默认'; break;
          }
        }
        break;

      default:
        break;
    }
    return result;
  }
}
