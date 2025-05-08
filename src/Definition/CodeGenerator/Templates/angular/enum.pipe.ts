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
//[@EnumBlocks]
      default:
        break;
    }
    return result;
  }
}

//@NgModule({
//  declarations: [EnumTextPipe], exports: [EnumTextPipe]
//})
//export class EnumTextPipeModule { }