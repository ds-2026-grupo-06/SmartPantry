import type { EstadoPantryItem } from './estado-pantry-item.enum';
import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdatePantryItemDto {
  idPropietario: string;
  nombreProducto: string;
  estado: EstadoPantryItem;
  fechaVencimiento?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
}

export interface PantryItemDto extends AuditedEntityDto<string> {
  idPropietario?: string;
  nombreProducto?: string;
  estado?: EstadoPantryItem;
  fechaVencimiento?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
}
