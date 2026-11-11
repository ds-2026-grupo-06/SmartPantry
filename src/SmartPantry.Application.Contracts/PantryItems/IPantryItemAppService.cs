using System;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using System.Collections.Generic;

namespace SmartPantry.PantryItems;
public interface IPantryItemAppService :
    ICrudAppService< //Defines CRUD methods
        PantryItemDto, //Used to show pantry items
        Guid, //Primary key of the pantry item entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdatePantryItemDto> //Used to create/update a pantry item
{
    Task<PantryItemDto> ActualizarEstadoAsync(Guid id, EstadoPantryItem estado);
    Task<PantryItemDto> ActualizarFechaVencimientoAsync(Guid id, DateTime fechaVencimiento);
    Task<PantryItemDto> ActualizarCantidadAsync(Guid id, int cantidad);
    Task<PantryItemDto> ActualizarUnidadAsync(Guid id, string unidad);
    Task<List<PantryItemDto>> ListarPorNombreAsync(string nombreProducto);
    Task EliminarItemAsync(Guid id);
}
