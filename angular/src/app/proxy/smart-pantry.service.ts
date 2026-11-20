import type { ProductDto } from './products/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SmartPantryService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/smart-pantry/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto[]>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/smart-pantry',
    },
    { apiName: this.apiName,...config });
}