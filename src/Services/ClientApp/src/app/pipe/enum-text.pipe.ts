// 该文件自动生成，会被覆盖更新
import { Injectable, Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
@Injectable({ providedIn: 'root' })
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): string {
    let result = '';
    switch (type) {
      case 'ActionStatus':
        {
          switch (value) {
            case 0: result = '未执行'; break;
            case 1: result = '执行中'; break;
            case 2: result = '成功'; break;
            case 3: result = '失败'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'CacheType':
        {
          switch (value) {
            case 0: result = 'Redis'; break;
            case 1: result = 'Memory'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'CommandType':
        {
          switch (value) {
            case 0: result = 'dto'; break;
            case 1: result = 'manager'; break;
            case 2: result = 'api'; break;
            case 3: result = 'protobuf'; break;
            case 4: result = 'clear'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'ControllerType':
        {
          switch (value) {
            case 0: result = '用户端'; break;
            case 1: result = '管理端'; break;
            case 2: result = '用户端和管理端'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'DBType':
        {
          switch (value) {
            case 0: result = 'PostgreSQL'; break;
            case 1: result = 'SQLServer'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'EntityKeyType':
        {
          switch (value) {
            case 0: result = ''; break;
            case 1: result = ''; break;
            case 2: result = ''; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'FrontType':
        {
          switch (value) {
            case 0: result = ''; break;
            case 1: result = 'Angular'; break;
            case 2: result = 'Blazor'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'GenSourceType':
        {
          switch (value) {
            case 0: result = '实体类'; break;
            case 1: result = 'Dto模型'; break;
            case 2: result = 'OpenAPI'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'GenStepType':
        {
          switch (value) {
            case 0: result = '模板生成'; break;
            case 1: result = '执行命令'; break;
            case 2: result = '执行脚本'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'OperationType':
        {
          switch (value) {
            case 0: result = 'get'; break;
            case 1: result = 'put'; break;
            case 2: result = 'post'; break;
            case 3: result = 'delete'; break;
            case 4: result = 'options'; break;
            case 5: result = 'head'; break;
            case 6: result = 'patch'; break;
            case 7: result = 'trace'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'ProjectType':
        {
          switch (value) {
            case 0: result = 'web服务'; break;
            case 1: result = '控制台应用'; break;
            case 2: result = '类库'; break;
            case 3: result = '模块'; break;
            case 4: result = '接口服务'; break;
            case 5: result = 'gPRC服务'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'RequestLibType':
        {
          switch (value) {
            case 0: result = 'angular http'; break;
            case 1: result = 'axios'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'SolutionType':
        {
          switch (value) {
            case 0: result = 'DotNet'; break;
            case 1: result = 'Node'; break;
            case 2: result = 'Empty'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'StringConvertType':
        {
          switch (value) {
            case 0: result = 'Guid'; break;
            case 1: result = '命名转换'; break;
            case 2: result = '编码'; break;
            case 3: result = '解码'; break;
            case 4: result = '加密'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'ValueType':
        {
          switch (value) {
            case 0: result = ''; break;
            case 1: result = ''; break;
            case 2: result = ''; break;
            case 3: result = ''; break;
            case 4: result = ''; break;
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
