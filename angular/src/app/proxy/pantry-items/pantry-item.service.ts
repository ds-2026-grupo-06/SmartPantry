import type { EstadoPantryItem } from './estado-pantry-item.enum';
import type { CreateUpdatePantryItemDto, PantryItemDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PantryItemService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  actualizarCantidad = (id: string, cantidad: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}/actualizar-cantidad`,
      params: { cantidad },
    },
    { apiName: this.apiName,...config });
  

  actualizarEstado = (id: string, estado: EstadoPantryItem, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}/actualizar-estado`,
      params: { estado },
    },
    { apiName: this.apiName,...config });
  

  actualizarFechaVencimiento = (id: string, fechaVencimiento: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}/actualizar-fecha-vencimiento`,
      params: { fechaVencimiento },
    },
    { apiName: this.apiName,...config });
  

  actualizarUnidad = (id: string, unidad: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}/actualizar-unidad`,
      params: { unidad },
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdatePantryItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/pantry-item',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/pantry-item/${id}`,
    },
    { apiName: this.apiName,...config });
  

  eliminarItem = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/pantry-item/${id}/eliminar-item`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PantryItemDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/pantry-item',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  listarPorNombre = (nombreProducto: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto[]>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/pantry-item/listar-por-nombre',
      params: { nombreProducto },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdatePantryItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PantryItemDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/pantry-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}