import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  barcode: string;
  name: string;
  brand: string;
  nutriScore: string;
  novaGroup: number;
}

export interface ProductDto extends AuditedEntityDto<string> {
  id?: string;
  barcode?: string;
  name?: string;
  brand?: string;
  nutriScore?: string;
  novaGroup?: number;
}
