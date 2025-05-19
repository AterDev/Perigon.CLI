import { ValueType } from '../../enum/models/value-type.model';
export interface ConfigData {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  key: string;
  value: string;
  valueType?: ValueType | null;

}
