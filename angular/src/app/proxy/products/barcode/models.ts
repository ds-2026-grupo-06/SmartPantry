import type { EntityDto } from '@abp/ng.core';

export interface ExternalProductDto extends EntityDto<string> {
  barcode?: string;
  name?: string;
  brand?: string;
  quantity?: string;
  imageUrl?: string;
  nutrients?: string;
}

export interface GetProductByBarcodeDto {
  barcode: string;
}
