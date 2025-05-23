import { NgModule, Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'toKeyValue' })
export class ToKeyValuePipe implements PipeTransform {
  transform(enumData: any): { key: string, value: number }[] {

    var result = Object.keys(enumData)
      .filter(key => isNaN(Number(key)))
      .map((key) => ({ key, value: enumData[key] }));

    return result;
  }
}

