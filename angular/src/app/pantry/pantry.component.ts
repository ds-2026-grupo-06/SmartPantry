import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule, ListService, PagedResultDto } from '@abp/ng.core';
import { PantryItemService, PantryItemDto } from '@proxy/pantry-items';

@Component({
  selector: 'app-pantry',
  standalone: true,
  imports: [CommonModule, CoreModule],
  providers: [ListService],
  templateUrl: './pantry.component.html'
})
export class PantryComponent implements OnInit {
  items = { items: [], totalCount: 0 } as PagedResultDto<PantryItemDto>;
  hasError = false;

  constructor(
    public readonly list: ListService,
    private pantryService: PantryItemService
  ) { }

  ngOnInit() {
    // skipHandleError oculta el popup rojo de ABP para que mostremos nuestro propio error
    const streamCreator = (query: any) => this.pantryService.getList(query, { skipHandleError: true });

    this.list.hookToQuery(streamCreator).subscribe({
      next: (response) => {
        this.items = response;
        this.hasError = false;
      },
      error: () => {
        this.hasError = true;
      }
    });
  }

  reintentar() {
    this.hasError = false;
    this.list.get(); // Acá sí usamos get() porque es un reintento manual
  }
}
