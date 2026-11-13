import { mapEnumToOptions } from '@abp/ng.core';

export enum EstadoPantryItem {
  Disponible = 0,
  Consumido = 1,
  Vencido = 2,
}

export const estadoPantryItemOptions = mapEnumToOptions(EstadoPantryItem);
